using MCApi.Utils;
using Mono.Unix;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LauncherGenerator;

public static class BuildLogic
{
    public static async Task LoadLibraries(IEnumerable<VersionManifest> manifests)
    {
        var libsToDownload = manifests
            .SelectMany(x => x.Libraries)
            .SelectMany(x => x.NeededDownloads)
            .Where(x => x.Value.Url != null)
            .Select(x => x.Value)
            .Distinct()
            .ToArray();

        await Task.WhenAll(libsToDownload
            .DownloadLibraries("data/libraries", (created, n) => { if (created) { Log.FileNew("data/libraries/" + n.LibraryPath); } }));

        await Task.WhenAll(
            manifests.Select(x => x.UnpackNatives("data/libraries", "data/versions/" + x.ID + "/natives"))
        );
    }
    public static async Task LoadClientJars(IEnumerable<VersionManifest> manifests)
    {
        await Task.WhenAll(
            manifests
                .DistinctBy(x => x.Client)
                .Select(async x =>
                {
                    string jarId = x.JarFrom?.ID ?? x.ID;
                    string path = "data/versions/" + jarId + "/" + jarId + ".jar";
                    if (await ((RemoteFile)x.Client).Save(new string[] { path }))
                        Log.FileNew(path);
                })
        );
    }
    public static async Task SaveVersionManifests(IEnumerable<VersionManifest> manifests)
    {
        await Task.WhenAll(
            manifests.Select(x =>
                Task.Run(async () =>
                {
                    var dir = $"data/versions/{x.ID}";
                    Directory.CreateDirectory(dir);
                    var path = $"{dir}/{x.ID}.json";
                    await File.WriteAllTextAsync(path, JsonSerializer.Serialize(x.DescribedBy, CommonJsonOptions.Options));
                    Log.FileNew(path);
                })
            ));
    }

    public static async Task SaveAssetIndexes(IEnumerable<VersionManifest> manifests)
    {
        await Task.WhenAll(
            manifests.Select(async x =>
            {
                AssetGroupIndex ai = await x.AssetGroup.GetIndex();
                Directory.CreateDirectory("data/assets/indexes");
                var path = $"data/assets/indexes/{x.AssetGroup.ID}.json";
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(ai.DescribedBy, CommonJsonOptions.Options));
                Log.FileNew(path);
            }));
    }

    public static async Task LoadAssetObjects(IEnumerable<VersionManifest> manifests)
    {
        var allAssetIndexes = await Task.WhenAll(
            manifests.Select(x => x.AssetGroup.GetIndex())
        );

        await allAssetIndexes
            .SelectMany(x => x.Objects)
            .DistinctBy(x => x.Hash)
            .DownloadObjects("data/assets/objects", (cr, obj) =>
            {
                if (cr)
                    Log.FileNew("data/assets/objects/" + obj.Hash.Substring(0, 2) + "/" + obj.Hash + " <- " + String.Join(", ", obj.Paths));
            });

        await Task.WhenAll(
            allAssetIndexes
                .Select(x => x.UnpackVirtuals("data/assets/objects", "data/assets/virtual/legacy"))
        );
    }
    public static async Task LoadLogConfigs(IEnumerable<VersionManifest> manifests)
    {
        await manifests
            .Where(x => x.LoggingSettings.ContainsKey("client"))
            .Select(x => x.LoggingSettings["client"].File)
            .Distinct()
            .DownloadGenericFiles(x => new string[] { "data/assets/log_configs/" + ((RemoteFile)x).Name },
            (cr, x) =>
            {
                if (cr)
                    Log.FileNew("data/assets/log_configs/" + ((RemoteFile)x).Name);
            });
    }
    public static async Task WriteScripts(Config cfg, TransformedTarget[] tv, MCDownload downloader)
    {
        foreach (var t in tv)
        {
            MCVersion v = downloader.GetRemoteVersion(t.VersionId);
            VersionManifest vm = await v.GetManifest();
            var argBuilder = new CommandLineArgumentBuilder();
            argBuilder.Add(vm.JavaArguments);

            GameArguments combined = new GameArguments(t.From.JVMArguments)
                + vm.JavaArguments
                //  (vm.LoggingSettings.ContainsKey("client") ? vm.LoggingSettings["client"].GameArgument : new GameArguments("")) +
                + new GameArguments("me.basiqueevangelist.launchergenerator.authhelper.MinecraftAuthHelper")
                + new GameArguments(vm.MainClass)
                + new GameArguments(cfg.Username)
                + new GameArguments(t.From.NewGameArguments)
                + vm.MinecraftArguments;
            AssetGroupIndex ai = await vm.AssetGroup.GetIndex();
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                ["auth_player_name"] = "@USERNAME",
                ["version_name"] = t.VersionId,
                ["game_directory"] = ".",
                ["assets_root"] = "../../assets",
                ["game_assets"] = "../../assets" + (ai.IsVirtual ? "/virtual/" + vm.AssetGroup.ID : ""),
                ["assets_index_name"] = vm.AssetGroup.ID,
                ["auth_uuid"] = "@UUID",
                ["auth_access_token"] = "@ACCESSTOKEN",
                ["user_type"] = "mojang",
                ["version_type"] = v.Type.ToString(),
                ["classpath"] = Classpath(vm),
                ["natives_directory"] = "../../versions/" + t.VersionId + "/natives",
                ["launcher_name"] = "LauncherGenerator",
                ["launcher_version"] = "0.1a",
                ["path"] = vm.LoggingSettings.ContainsKey("client") ? "../../assets/log_configs/" + ((RemoteFile)vm.LoggingSettings["client"].File).Name : ""
            };
            string[] cargs = combined.Process(variables, Array.Empty<string>());
            string fname = "data/" + t.From.Name + (Environment.OSVersion.Platform == PlatformID.Win32NT ? ".bat" : ".sh");
            using (FileStream fs = File.Open(fname, System.IO.FileMode.Create, FileAccess.Write, FileShare.Read))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    sw.WriteLine("title Minecraft " + t.VersionId + " launch script");
                    sw.WriteLine("cd %~dp0");
                }
                else
                {
                    sw.WriteLine("#!/bin/sh");
                    sw.WriteLine("printf \"\\033]0;Minecraft " + t.VersionId + " launch script\\007\"");
                    sw.WriteLine("cd $(cd `dirname $0` && pwd)");
                }
                sw.WriteLine("cd profiles/" + t.From.Profile);
                sw.WriteLine(t.From.JavaPath.Replace("\\", "\\\\") + " " + GameArguments.FoldArgs(cargs));
                // sw.WriteLine("pause");
            }
            Log.FileNew(fname);
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                UnixFileInfo ufi = new UnixFileInfo(fname);
                ufi.FileAccessPermissions |= FileAccessPermissions.UserExecute | FileAccessPermissions.GroupRead | FileAccessPermissions.OtherExecute;
            }
        }

        var assembly = typeof(Program).Assembly;
        using (Stream jarIn = assembly.GetManifestResourceStream("LauncherGenerator.MCAuthHelper.jar") ?? throw new NotImplementedException())
        using (FileStream jarOut = File.Open("data/MCAuthHelper.jar", FileMode.Create, FileAccess.Write, FileShare.Delete))
            await jarIn.CopyToAsync(jarOut);
    }

    public static async Task WriteLauncherProfiles()
    {
        JObject obj = new JObject();
        obj.Add("_comment", "This is a dummy launcher_profiles.json file. It exists so that Optifine can install successfully.");
        obj.Add("profiles", new JObject());
        await File.WriteAllTextAsync("data/launcher_profiles.json", obj.ToString());
        Log.FileNew("data/launcher_profiles.json");
    }

    static string Classpath(VersionManifest vm)
    {
        List<string> entr = vm.Libraries.Where(x => x.IsNeeded).SelectMany(x => x.NeededDownloads).Select(x => "../../libraries/" + x.Value.LibraryPath).ToList();
        string jarId = vm.JarFrom?.ID ?? vm.ID;
        entr.Add("../../versions/" + jarId + "/" + jarId + ".jar");
        entr.Add("../../MCAuthHelper.jar");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            return string.Join(";", entr);
        else
            return string.Join(":", entr);
    }
}