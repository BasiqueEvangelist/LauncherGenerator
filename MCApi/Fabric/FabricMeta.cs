using System.Web;
namespace MCApi.Fabric;

public static class FabricMeta
{
    public const string API_URL = "https://meta.fabricmc.net";

    public static Task<CompatibleSetupForVersion[]> GetSetupsForVersion(string game_version)
    {
        string game_version_u = HttpUtility.UrlEncode(game_version);
        return MCHttpHelper.Get<CompatibleSetupForVersion[]>(API_URL + "/v2/versions/loader/" + game_version_u);
    }

    public static Task<VersionManifestDefinition> GetManifestFor(string game_version, string loader_version)
    {
        string game_version_u = HttpUtility.UrlEncode(game_version);
        string loader_version_u = HttpUtility.UrlEncode(loader_version);
        return MCHttpHelper.Get<VersionManifestDefinition>($"{API_URL}/v2/versions/loader/{game_version_u}/{loader_version_u}/profile/json");
    }
}