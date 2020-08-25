using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using MCApi;
using Newtonsoft.Json;

namespace LauncherGenerator
{
    public class LocalVersionResolver : IVersionResolver
    {
        public Task<IEnumerable<VersionDefinition>> GetAllVersions()
        {
            List<VersionDefinition> vl = new List<VersionDefinition>();
            JsonSerializer js = new JsonSerializer();
            foreach (string s in Directory.EnumerateDirectories("data/versions"))
            {
                string vid = Path.GetFileName(s);
                if (File.Exists(Path.Combine(s, vid + ".json")))
                    using (var fs = File.OpenRead(Path.Combine(s, vid + ".json")))
                    using (StreamReader sr = new StreamReader(fs))
                    using (JsonTextReader jr = new JsonTextReader(sr))
                        vl.Add(js.Deserialize<VersionManifestDefinition>(jr));
            }
            return Task.FromResult(vl as IEnumerable<VersionDefinition>);
        }

        public Task<AssetGroupIndexDefinition> GetAssetIndex(AssetGroup id)
        {
            JsonSerializer js = new JsonSerializer();
            if (File.Exists(Path.Combine("data", "assets", "indexes", id.ID + ".json")))
                using (var fs = File.OpenRead(Path.Combine("data", "assets", "indexes", id.ID + ".json")))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonTextReader jr = new JsonTextReader(sr))
                    return Task.FromResult(js.Deserialize<AssetGroupIndexDefinition>(jr));
            throw new MCDownloadException("Couldn't find assets in local files");
        }

        public Task<VersionManifestDefinition> GetVersion(MCVersion v)
        {
            JsonSerializer js = new JsonSerializer();
            if (File.Exists(Path.Combine("data", "versions", v.ID, v.ID + ".json")))
                using (var fs = File.OpenRead(Path.Combine("data", "versions", v.ID, v.ID + ".json")))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonTextReader jr = new JsonTextReader(sr))
                    return Task.FromResult(js.Deserialize<VersionManifestDefinition>(jr));
            throw new MCDownloadException("Couldn't find version in local files");
        }
    }
}