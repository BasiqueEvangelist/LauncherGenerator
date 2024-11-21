namespace LauncherGenerator;

public static class ProgramTasks
{
    private static async Task<(VersionManifest[], MCDownload)> InitData(Config cfg, TransformedTarget[] transformedTargets)
    {
        MCDownload downloader = new MCDownload(new CombinedVersionResolver(
            new LocalVersionResolver(),
            new RemoteVersionResolver()
        ));
        
        await downloader.Init();

        Log.Step("Initialized MCDownload");

        foreach (Target t in cfg.Targets)
            Directory.CreateDirectory("data/profiles/" + t.Profile);

        var manifests = await Task.WhenAll(
            transformedTargets
            .Distinct()
            .Select(x => downloader.GetRemoteVersion(x.VersionId).GetManifest())
        );

        Log.Step("Downloaded manifests");

        return (manifests, downloader);
    }

    private static Task<TransformedTarget[]> TransformTargets(Target[] x)
    {
        AllTransformer all = new AllTransformer();
        return Task.WhenAll(x.Select(y => all.Transform(y)));
    }

    public static async Task CollectGarbage(Config cfg)
    {
        Log.Step("Collecting garbage");
        var tv = await TransformTargets(cfg.Targets);
        var (manifests, downloader) = await InitData(cfg, tv);

        await GCLogic.CollectAssets(manifests);
        GCLogic.CollectScripts(cfg.Targets);
        GCLogic.CollectLibraries(manifests);
        Log.Step("Done");
    }

    public static async Task Build(Config cfg)
    {
        Log.Step("Generating launcher");

        var tv = await TransformTargets(cfg.Targets);
        var (manifests, downloader) = await InitData(cfg, tv);

        await Task.WhenAll(
            BuildLogic.LoadLibraries(manifests),
            BuildLogic.LoadClientJars(manifests),
            BuildLogic.SaveVersionManifests(manifests),
            BuildLogic.SaveAssetIndexes(manifests),
            BuildLogic.LoadAssetObjects(manifests),
            BuildLogic.LoadLogConfigs(manifests),
            BuildLogic.WriteLauncherProfiles()
        );

        await BuildLogic.WriteScripts(cfg, tv, downloader);
        Log.Step("Done!");
    }
}