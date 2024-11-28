using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
    public Dictionary<string, IGameArgument>? ComplexArguments;
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
    public class DownloadsBlock
    {
        [JsonPropertyName("artifact")]
        public RemoteFileDefinition? Artifact;
        [JsonPropertyName("classifiers")]
        public Dictionary<string, RemoteFileDefinition>? Classifiers;
    }

    [JsonPropertyName("name")]
    public required string Name;
    [JsonPropertyName("downloads")]
    public DownloadsBlock? Downloads;
    [JsonPropertyName("url")]
    public Uri? Url;
    [JsonPropertyName("rules")]
    public MCRule[]? Rules;
    [JsonPropertyName("natives")]
    public Dictionary<string, string>? NativeNames;
    [JsonPropertyName("extract")]
    public ExtractBlock? ExtractionSettings;
}

[JsonConverter(typeof(GameArgumentJsonConverter))]
public interface IGameArgument;

public class GameArgumentJsonConverter : JsonConverter<IGameArgument>
{
    public override IGameArgument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new SimpleArgument(reader.GetString()!);
        if (reader.TokenType == JsonTokenType.StartArray)
            return new ListArgument(JsonSerializer.Deserialize<List<IGameArgument>>(ref reader, options)!);

        return JsonSerializer.Deserialize<ComplexArgument>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IGameArgument value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SimpleArgument sa:
                writer.WriteStringValue(sa.Value);
                break;
            case ComplexArgument ca:
                JsonSerializer.Serialize(writer, ca, options);
                break;
            case ListArgument la:
                JsonSerializer.Serialize(writer, la.Values, options);
                break;
            default: throw new JsonException();
        };
    }
}

public record SimpleArgument(string Value) : IGameArgument;

public class ComplexArgument : IGameArgument
{
    [JsonPropertyName("value")]
    public required IGameArgument Value;
    [JsonPropertyName("rules")]
    public required MCRule[] Rules;
}

public record class ListArgument(List<IGameArgument> Values) : IGameArgument
{
    public static ListArgument FromBuiltString(string commandline)
    {
        var parts = StringExtensions.SplitCommandLine(commandline);
        var args = parts.Select(s => new SimpleArgument(s) as IGameArgument);
        return new ListArgument(args.ToList());
    }
};

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

    public bool Active(string[]? features = null)
    {
        if (OS != null)
        {
            if (OS.Name is string name && name != SystemInfo.CurrentPlatform)
                return false;
            if (OS.VersionRegex != null)
            {
                if (!new Regex(OS.VersionRegex).IsMatch(Environment.OSVersion.VersionString))
                    return false;
            }
            if (OS.Architecture != null)
            {
                if (Environment.Is64BitOperatingSystem && OS.Architecture == "x86")
                    return false;
            }
        }

        if (RequiredFeatures != null)
        {
            if (features is null) features = [];
            if (RequiredFeatures.Any(f => features.Contains(f.Key) != f.Value)) ;
            return false;
        }
        return true;
    }
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
