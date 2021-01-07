using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCApi
{
    public class CombinedVersionResolver : IVersionResolver
    {
        private List<IVersionResolver> stack;

        public CombinedVersionResolver(params IVersionResolver[] resolvers)
        {
            stack = new List<IVersionResolver>(resolvers);
        }

        public void AddResolver(IVersionResolver resv)
        {
            stack.Add(resv);
        }

        public async Task<IEnumerable<VersionDefinition>> GetAllVersions()
        {
            Dictionary<string, VersionDefinition> versions = new Dictionary<string, VersionDefinition>();
            foreach (IVersionResolver resolver in stack)
                foreach (VersionDefinition version in await resolver.GetAllVersions())
                    versions.TryAdd(version.ID, version);

            return versions.Values;
        }

        public async Task<AssetGroupIndexDefinition> GetAssetIndex(AssetGroup ag)
        {
            foreach (IVersionResolver resolver in stack)
            {
                try
                {
                    return await resolver.GetAssetIndex(ag);
                }
                catch (MCDownloadException)
                {
                    // Couldn't get version, continuing to next resolver    
                }
            }
            throw new MCDownloadException("Could not find asset index \"" + ag.ID + "\"");
        }

        public async Task<VersionManifestDefinition> GetVersion(MCVersion v)
        {
            foreach (IVersionResolver resolver in stack)
            {
                try
                {
                    return await resolver.GetVersion(v);
                }
                catch (MCDownloadException)
                {
                    // Couldn't get version, continuing to next resolver    
                }
            }
            throw new MCDownloadException("Could not find version \"" + v + "\"");
        }
    }
}