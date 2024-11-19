namespace MCApi;

// private static async Task getRemoteVersions()
// {
//     if (vlloaded) return;
//     VersionList vl = await MCHttpHelper.Get<VersionList>("https://launchermeta.mojang.com/mc/game/version_manifest.json");
//     lock (vcachelock)
//     {
//         foreach (VersionDefinition vd in vl.Versions)
//         {
//             if (!versionCache.Any(x => x.ID == vd.ID))
//             {
//                 versionCache.Add(new MCVersion(vd));
//             }
//         }
//     }
//     vlloaded = true;
// }
public class RemoteVersionResolver : IVersionResolver
{
    public async Task<IEnumerable<VersionDefinition>> GetAllVersions()
    {
        return (await MCHttpHelper.Get<VersionList>("https://launchermeta.mojang.com/mc/game/version_manifest.json")).Versions;
    }

    public Task<VersionManifestDefinition> GetVersion(MCVersion v)
    {
        if (v.Url == null)
            throw new MCDownloadException("Version doesn't have any URL");
        return MCHttpHelper.Get<VersionManifestDefinition>(v.Url);
    }

    public Task<AssetGroupIndexDefinition> GetAssetIndex(AssetGroup ag)
    {
        if (ag.DescribedBy.Url == null)
            throw new MCDownloadException("Asset group doesn't have any url.");
        return MCHttpHelper.Get<AssetGroupIndexDefinition>(ag.DescribedBy.Url);
    }
}