using System.Text.Json.Serialization;

namespace MCApi.Fabric;

public class LoaderVersion
{
    [JsonPropertyName("separator")]
    public required string Separator;
    [JsonPropertyName("build")]
    public required int Build;
    [JsonPropertyName("maven")]
    public required string MavenId;
    [JsonPropertyName("version")]
    public required string Version;
    [JsonPropertyName("stable")]
    public required bool Stable;
}
public class IntermediaryVersion
{
    [JsonPropertyName("maven")]
    public required string MavenId;
    [JsonPropertyName("version")]
    public required string Version;
    [JsonPropertyName("stable")]
    public required bool Stable;
}
public class CompatibleSetupForVersion
{
    [JsonPropertyName("loader")]
    public required LoaderVersion Loader;
    [JsonPropertyName("intermediary")]
    public required IntermediaryVersion Intermediary;
}