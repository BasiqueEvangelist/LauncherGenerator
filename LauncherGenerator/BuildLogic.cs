using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCApi;
using Mono.Unix;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LauncherGenerator
{
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
            JsonSerializer js = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            await Task.WhenAll(
                manifests.Select(x =>
                    Task.Run(() =>
                    {
                        Directory.CreateDirectory("data/versions/" + x.ID + "/");
                        using (FileStream fs = File.Open("data/versions/" + x.ID + "/" + x.ID + ".json", FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(fs))
                        using (JsonTextWriter jw = new JsonTextWriter(sw))
                            js.Serialize(jw, x.DescribedBy);
                        Log.FileNew("data/versions/" + x.ID + "/" + x.ID + ".json");
                    })
                ));
        }
        public static async Task SaveAssetIndexes(IEnumerable<VersionManifest> manifests)
        {
            JsonSerializer js = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            await Task.WhenAll(
                manifests.Select(async x =>
                {
                    AssetGroupIndex ai = await x.AssetGroup.GetIndex();
                    Directory.CreateDirectory("data/assets/indexes");
                    using (FileStream fs = File.Open("data/assets/indexes/" + x.AssetGroup.ID + ".json", FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (StreamWriter sw = new StreamWriter(fs))
                    using (JsonTextWriter jw = new JsonTextWriter(sw))
                        js.Serialize(jw, ai.DescribedBy);
                    Log.FileNew("data/assets/indexes/" + x.AssetGroup.ID + ".json");
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
                    .Select(x => x.UnpackVirtuals("data/assets/objects", "data/assets/virtuals/" + x))
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
                MCVersion v = downloader.GetRemoteVersion(t.VersionID);
                VersionManifest vm = await v.GetManifest();
                GameArguments combined = new GameArguments(t.From.JVMArguments)
                    + vm.JavaArguments
                    //  (vm.LoggingSettings.ContainsKey("client") ? vm.LoggingSettings["client"].GameArgument : new GameArguments("")) +
                    + new GameArguments(vm.MainClass)
                    + new GameArguments(t.From.NewGameArguments)
                    + vm.MinecraftArguments;
                AssetGroupIndex ai = await vm.AssetGroup.GetIndex();
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    ["auth_player_name"] = "@USERNAME",
                    ["version_name"] = t.VersionID,
                    ["game_directory"] = ".",
                    ["assets_root"] = "../../assets",
                    ["game_assets"] = "../../assets" + (ai.IsVirtual ? "/virtual/" + vm.AssetGroup.ID : ""),
                    ["assets_index_name"] = vm.AssetGroup.ID,
                    ["auth_uuid"] = "@UUID",
                    ["auth_access_token"] = "@ACCESSTOKEN",
                    ["user_type"] = "mojang",
                    ["version_type"] = v.Type.ToString(),
                    ["classpath"] = Classpath(vm),
                    ["natives_directory"] = "../../versions/" + t.VersionID + "/natives",
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
                        sw.WriteLine("title Minecraft " + t.VersionID + " launch script");
                        sw.WriteLine("cd %~dp0");
                    }
                    else
                    {
                        sw.WriteLine("#!/bin/bash");
                        sw.WriteLine("printf \"\\033]0;Minecraft " + t.VersionID + " launch script\\007\"");
                        sw.WriteLine("cd $(cd `dirname $0` && pwd)");
                    }
                    sw.WriteLine("cd profiles/" + t.From.Profile);
                    var ahline =
                     (Environment.OSVersion.Platform == PlatformID.Win32NT ? "..\\..\\mcauthhelper.exe " : "../../mcauthhelper ") + cfg.Username + " ";
                    sw.WriteLine(ahline + t.From.JavaPath.Replace("\\", "\\\\") + " " + GameArguments.FoldArgs(cargs));
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
            var exename = Environment.OSVersion.Platform != PlatformID.Win32NT
                    ? "mcauthhelper"
                    : "mcauthhelper.exe";
            using (Stream exampleIn = assembly.GetManifestResourceStream("LauncherGenerator." + exename) ?? throw new NotImplementedException())
            using (FileStream exampleOut = File.Open("data/" + exename, FileMode.Create, FileAccess.Write, FileShare.Delete))
                await exampleIn.CopyToAsync(exampleOut);
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                UnixFileInfo ufi = new UnixFileInfo("data/" + exename);
                ufi.FileAccessPermissions |= FileAccessPermissions.UserExecute | FileAccessPermissions.GroupRead | FileAccessPermissions.OtherExecute;
            }
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
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return string.Join(";", entr);
            else
                return string.Join(":", entr);
        }
    }
}