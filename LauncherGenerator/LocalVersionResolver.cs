using System.Text.Json;
using MCApi.Utils;

namespace LauncherGenerator;

public class LocalVersionResolver : IVersionResolver
{
    public async Task<IEnumerable<VersionDefinition>> GetAllVersions()
    {
        List<VersionDefinition> vl = new List<VersionDefinition>();
        foreach (string s in Directory.EnumerateDirectories("data/versions"))
        {
            string vid = Path.GetFileName(s);
            if (File.Exists(Path.Combine(s, vid + ".json")))
                using (var fs = File.OpenRead(Path.Combine(s, vid + ".json")))
                    vl.Add((await JsonSerializer.DeserializeAsync<VersionManifestDefinition>(fs, CommonJsonOptions.Options))!);
        }
        return vl;
    }

    public async Task<AssetGroupIndexDefinition> GetAssetIndex(AssetGroup id)
    {
        if (File.Exists(Path.Combine("data", "assets", "indexes", id.ID + ".json")))
            using (var fs = File.OpenRead(Path.Combine("data", "assets", "indexes", id.ID + ".json")))
                return await JsonSerializer.DeserializeAsync<AssetGroupIndexDefinition>(fs, CommonJsonOptions.Options);
        
        throw new MCDownloadException("Couldn't find assets in local files");
    }

    public async Task<VersionManifestDefinition> GetVersion(MCVersion v)
    {
        if (File.Exists(Path.Combine("data", "versions", v.ID, v.ID + ".json")))
            using (var fs = File.OpenRead(Path.Combine("data", "versions", v.ID, v.ID + ".json")))
                return await JsonSerializer.DeserializeAsync<VersionManifestDefinition>(fs, CommonJsonOptions.Options);
        
        throw new MCDownloadException("Couldn't find version in local files");
    }
}