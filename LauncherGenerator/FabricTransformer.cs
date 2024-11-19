using MCApi.Fabric;
using Newtonsoft.Json;

namespace LauncherGenerator
{
    public class FabricTransformer : ITargetTransformer
    {
        public async Task<TransformedTarget> Transform(Target from)
        {
            JsonSerializer js = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            var setups = await FabricMeta.GetSetupsForVersion(from.VersionID);
            var loader_v = setups[0].Loader.Version;
            var manifest = await FabricMeta.GetManifestFor(from.VersionID, loader_v);
            var newVid = manifest.ID;
            Directory.CreateDirectory($"data/versions/{newVid}");
            using (FileStream fs = File.Open($"data/versions/{newVid}/{newVid}.json", FileMode.Create, FileAccess.Write, FileShare.Delete))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonTextWriter jw = new JsonTextWriter(sw))
                js.Serialize(jw, manifest);
            File.Open($"data/versions/{newVid}/{newVid}.jar", FileMode.Create, FileAccess.Write, FileShare.ReadWrite).Dispose();

            return new TransformedTarget { From = from, VersionID = newVid };
        }
    }
}