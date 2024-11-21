using System.Text.Json;
using MCApi.Fabric;
using MCApi.Utils;

namespace LauncherGenerator;

public class QuiltTransformer : ITargetTransformer
{
    public async Task<TransformedTarget> Transform(Target from)
    {
        var setups = await QuiltMeta.GetSetupsForVersion(from.VersionID);
        var loader_v = setups[0].Loader.Version;
        var manifest = await QuiltMeta.GetManifestFor(from.VersionID, loader_v);
        var newVid = manifest.ID;
        Directory.CreateDirectory($"data/versions/{newVid}");
        
        using (FileStream fs = File.Open($"data/versions/{newVid}/{newVid}.json", FileMode.Create, FileAccess.Write,
                   FileShare.Delete))
            await JsonSerializer.SerializeAsync(fs, manifest, CommonJsonOptions.Options);;
        
        File.Open($"data/versions/{newVid}/{newVid}.jar", FileMode.Create, FileAccess.Write, FileShare.ReadWrite).Dispose();

        return new TransformedTarget(from, newVid);
    }
}