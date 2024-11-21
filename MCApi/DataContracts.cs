using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MCApi;

internal class SnakeCaseEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.SnakeCaseLower, false)
    where T : struct, Enum;

[JsonConverter(typeof(SnakeCaseEnumConverter<VersionType>))]
public enum VersionType
{
    Release, // release
    Snapshot, // snapshot
    OldAlpha, // old_alpha
    OldBeta // old_beta
}

public class VersionList
{
    [JsonPropertyName("latest")]
    public required Dictionary<VersionType, string> LatestVersion;
    [JsonPropertyName("versions")]
    public required VersionDefinition[] Versions;
}
public class VersionDefinition
{
    [JsonPropertyName("id")]
    public required string ID;
    [JsonPropertyName("type")]
    public required VersionType Type;
    [JsonPropertyName("url")]
    public Uri? Url;
    [JsonPropertyName("time")]
    public required DateTime Time;
    [JsonPropertyName("releaseTime")]
    public required DateTime ReleaseTime;
}
public class VersionManifestDefinition : VersionDefinition
{
    [JsonPropertyName("minimumLauncherVersion")]
    public int? MinimumLauncherVersion;
    [JsonPropertyName("mainClass")]
    public required string MainClass;
    [JsonPropertyName("logging")]
    public Dictionary<string, LoggingDefinition>? LoggingSettings;
    [JsonPropertyName("minecraftArguments")]
    public string? SimpleArguments;
    [JsonPropertyName("arguments")]
    public Dictionary<string, List<JsonValue>>? ComplexArguments;
    [JsonPropertyName("downloads")]
    public Dictionary<string, RemoteFileDefinition>? Downloads;
    [JsonPropertyName("assets")]
    public string? AssetGroupID;
    [JsonPropertyName("assetIndex")]
    public AssetGroupDefinition? AssetGroup;
    [JsonPropertyName("libraries")]
    public required LibraryDefinition[] Libraries;
    [JsonPropertyName("jar")]
    public string? JarFrom;
    [JsonPropertyName("inheritsFrom")]
    public string? InheritsFrom;

}
public class LibraryDefinition
{
    public class ExtractBlock
    {
        [JsonPropertyName("exclude")]
        public required string[] Exclude;
    }

    [JsonPropertyName("name")]
    public required string Name;
    [JsonPropertyName("downloads")]
    public Dictionary<string, JsonObject>? Downloads;
    [JsonPropertyName("url")]
    public Uri? Url;
    [JsonPropertyName("rules")]
    public MCRule[]? Rules;
    [JsonPropertyName("natives")]
    public Dictionary<string, string>? NativeNames;
    [JsonPropertyName("extract")]
    public ExtractBlock? ExtractionSettings;
}

public interface IGameArgument;

public class GameArgumentJsonConverter : JsonConverter<IGameArgument>
{
    public override IGameArgument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new SimpleArgument(reader.GetString()!);
        
        return JsonSerializer.Deserialize<ComplexArgument>(reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IGameArgument value, JsonSerializerOptions options)
    {
        if (value is SimpleArgument) {
            
        }
    }
}

public record SimpleArgument(string Value) : IGameArgument;

public class ComplexArgument : IGameArgument
{
    [JsonPropertyName("value")]
    public required JsonValue Value;
    [JsonPropertyName("rules")]
    public required MCRule[] Rules;
}
public class MCRule
{
    [JsonConverter(typeof(SnakeCaseEnumConverter<RuleAction>))]
    public enum RuleAction
    {
        Allow,
        Disallow
    }
    public class OSBlock
    {
        [JsonPropertyName("name")]
        public string? Name;
        [JsonPropertyName("version")]
        public string? VersionRegex;
        [JsonPropertyName("arch")]
        public string? Architecture;
    }

    [JsonPropertyName("action")]
    public required RuleAction Action;
    [JsonPropertyName("os")]
    public OSBlock? OS;
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? RequiredFeatures;

}
public class AssetGroupDefinition : RemoteFileDefinition
{
    [JsonPropertyName("totalSize")]
    public required int TotalSize;
    [JsonPropertyName("id")]
    public required string ID;
}
public class AssetGroupIndexDefinition
{
    [JsonPropertyName("objects")]
    public required Dictionary<string, AssetInfo> Objects;
    [JsonPropertyName("map_to_resources")]
    public bool IsVirtual = false;
}
public class AssetInfo
{
    [JsonPropertyName("hash")]
    public required string Hash;
    [JsonPropertyName("size")]
    public required int Size;
}
public class LoggingDefinition
{
    [JsonPropertyName("type")]
    public required string Type;
    [JsonPropertyName("argument")]
    public required string GameArgument;
    [JsonPropertyName("file")]
    public required RemoteFileDefinition File;
}
public class RemoteFileDefinition
{
    [JsonPropertyName("url")]
    public Uri? Url;
    [JsonPropertyName("sha1")]
    public string? SHA1;
    [JsonPropertyName("hash")]
    public string? Hash;
    [JsonPropertyName("path")]
    public string? Path;
    [JsonPropertyName("size")]
    public int Size = 0;
}
