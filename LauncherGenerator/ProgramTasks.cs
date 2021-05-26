using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCApi;

namespace LauncherGenerator
{
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

            var manifests =
                await transformedTargets
                .ToAsyncEnumerable()
                .Distinct()
                .SelectAwait(async x =>
                  await downloader.GetRemoteVersion(x.VersionID).GetManifest()).ToArrayAsync();

            Log.Step("Downloaded manifests");

            return (manifests, downloader);
        }

        private static IAsyncEnumerable<TransformedTarget> TransformTargets(Target[] x)
        {
            AllTransformer all = new AllTransformer();
            return x.ToAsyncEnumerable().SelectAwait(async y => await all.Transform(y));
        }

        public static async Task CollectGarbage(Config cfg)
        {
            Log.Step("Collecting garbage");
            var tv = await TransformTargets(cfg.Targets).ToArrayAsync();
            var (manifests, downloader) = await InitData(cfg, tv);

            await Task.WhenAll(new Task[]
            {
                GCLogic.CollectAssets(manifests),
            });
            GCLogic.CollectScripts(cfg.Targets);
            GCLogic.CollectLibraries(manifests);
            Log.Step("Done");
        }

        public static async Task Build(Config cfg)
        {
            Log.Step("Generating launcher");

            var tv = await TransformTargets(cfg.Targets).ToArrayAsync();
            var (manifests, downloader) = await InitData(cfg, tv);

            await Task.WhenAll(new Task[]
            {
                BuildLogic.LoadLibraries(manifests),
                BuildLogic.LoadClientJars(manifests),
                BuildLogic.SaveVersionManifests(manifests),
                BuildLogic.SaveAssetIndexes(manifests),
                BuildLogic.LoadAssetObjects(manifests),
                BuildLogic.LoadLogConfigs(manifests),
                BuildLogic.WriteLauncherProfiles()
            });

            await BuildLogic.WriteScripts(cfg, tv, downloader);
            Log.Step("Done!");
        }
    }
}