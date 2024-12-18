namespace MCApi;

public interface IVersionResolver
{
    Task<IEnumerable<VersionDefinition>> GetAllVersions();
    Task<VersionManifestDefinition> GetVersion(MCVersion v);
    Task<AssetGroupIndexDefinition> GetAssetIndex(AssetGroup ag);
}