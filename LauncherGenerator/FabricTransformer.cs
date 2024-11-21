using System.Text.Json;
using MCApi.Fabric;
using MCApi.Utils;

namespace LauncherGenerator;

public class FabricTransformer : ITargetTransformer
{
    public async Task<TransformedTarget> Transform(Target from)
    {
        var setups = await FabricMeta.GetSetupsForVersion(from.VersionID);
        var loader_v = setups[0].Loader.Version;
        var manifest = await FabricMeta.GetManifestFor(from.VersionID, loader_v);
        var newVid = manifest.ID;
        Directory.CreateDirectory($"data/versions/{newVid}");
        
        using (FileStream fs = File.Open($"data/versions/{newVid}/{newVid}.json", FileMode.Create, FileAccess.Write,
                   FileShare.Delete))
            await JsonSerializer.SerializeAsync(fs, manifest, CommonJsonOptions.Options);
            
        await File.Open($"data/versions/{newVid}/{newVid}.jar", FileMode.Create, FileAccess.Write, FileShare.ReadWrite).DisposeAsync();

        return new TransformedTarget { From = from, VersionID = newVid };
    }
}