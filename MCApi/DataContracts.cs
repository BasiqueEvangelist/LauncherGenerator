using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

#nullable disable

namespace MCApi;

[JsonConverter(typeof(StringEnumConverter))]
public enum VersionType
{
    release,
    snapshot,
    old_alpha,
    old_beta
}
public class VersionList
{
    [JsonProperty(PropertyName = "latest", Required = Required.Always)]
    public Dictionary<VersionType, string> LatestVersion;
    [JsonProperty(PropertyName = "versions", Required = Required.Always)]
    public VersionDefinition[] Versions;
}
public class VersionDefinition
{
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public string ID;
    [JsonProperty(PropertyName = "type", Required = Required.Always)]
    public VersionType Type;
    [JsonProperty(PropertyName = "url", Required = Required.Default)]
    public Uri Url;
    [JsonProperty(PropertyName = "time", Required = Required.Always)]
    public DateTime Time;
    [JsonProperty(PropertyName = "releaseTime", Required = Required.Always)]
    public DateTime ReleaseTime;
}
public class VersionManifestDefinition : VersionDefinition
{
    [JsonProperty(PropertyName = "minimumLauncherVersion", Required = Required.Default)]
    public int MinimumLauncherVersion;
    [JsonProperty(PropertyName = "mainClass", Required = Required.Always)]
    public string MainClass;
    [JsonProperty(PropertyName = "logging", Required = Required.Default)]
    public Dictionary<string, LoggingDefinition> LoggingSettings;
    [JsonProperty(PropertyName = "minecraftArguments", Required = Required.Default)]
    public string SimpleArguments;
    [JsonProperty(PropertyName = "arguments", Required = Required.Default)]
    public Dictionary<string, List<JToken>> ComplexArguments;
    [JsonProperty(PropertyName = "downloads", Required = Required.Default)]
    public Dictionary<string, RemoteFileDefinition> Downloads;
    [JsonProperty(PropertyName = "assets", Required = Required.Default)]
    public string AssetGroupID;
    [JsonProperty(PropertyName = "assetIndex", Required = Required.Default)]
    public AssetGroupDefinition AssetGroup;
    [JsonProperty(PropertyName = "libraries", Required = Required.Always)]
    public LibraryDefinition[] Libraries;
    [JsonProperty(PropertyName = "jar", Required = Required.Default)]
    public string JarFrom;
    [JsonProperty(PropertyName = "inheritsFrom", Required = Required.Default)]
    public string InheritsFrom;

}
public class LibraryDefinition
{
    public class ExtractBlock
    {
        [JsonProperty(PropertyName = "exclude", Required = Required.Always)]
        public string[] Exclude;

    }

    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public string Name;
    [JsonProperty(PropertyName = "downloads", Required = Required.Default)]
    public Dictionary<string, JObject> Downloads;
    [JsonProperty(PropertyName = "url", Required = Required.Default)]
    public Uri Url;
    [JsonProperty(PropertyName = "rules", Required = Required.Default)]
    public MCRule[] Rules;
    [JsonProperty(PropertyName = "natives", Required = Required.Default)]
    public Dictionary<string, string> NativeNames;
    [JsonProperty(PropertyName = "extract", Required = Required.Default)]
    public ExtractBlock ExtractionSettings;
}
public class ComplexArgument
{
    [JsonProperty(PropertyName = "value", Required = Required.Always)]
    public JToken Value;
    [JsonProperty(PropertyName = "rules", Required = Required.Always)]
    public MCRule[] Rules;
}
public class MCRule
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleAction
    {
        allow,
        disallow
    }
    public class OSBlock
    {
        [JsonProperty(PropertyName = "name", Required = Required.Default)]
        public string Name;
        [JsonProperty(PropertyName = "version", Required = Required.Default)]
        public string VersionRegex;
        [JsonProperty(PropertyName = "arch", Required = Required.Default)]
        public string Architecture;
    }

    [JsonProperty(PropertyName = "action", Required = Required.Always)]
    public RuleAction Action;
    [JsonProperty(PropertyName = "os", Required = Required.Default)]
    public OSBlock OS;
    [JsonProperty(PropertyName = "features", Required = Required.Default)]
    public Dictionary<string, bool> RequiredFeatures;

}
public class AssetGroupDefinition : RemoteFileDefinition
{
    [JsonProperty(PropertyName = "totalSize", Required = Required.Always)]
    public int TotalSize;
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public string ID;
}
public class AssetGroupIndexDefinition
{
    [JsonProperty(PropertyName = "objects", Required = Required.Always)]
    public Dictionary<string, AssetInfo> Objects;
    [JsonProperty(PropertyName = "map_to_resources", Required = Required.Default)]
    public bool IsVirtual;
}
public class AssetInfo
{
    [JsonProperty(PropertyName = "hash", Required = Required.Always)]
    public string Hash;
    [JsonProperty(PropertyName = "size", Required = Required.Always)]
    public int Size;
}
public class LoggingDefinition
{
    [JsonProperty(PropertyName = "type", Required = Required.Always)]
    public string Type;
    [JsonProperty(PropertyName = "argument", Required = Required.Always)]
    public string GameArgument;
    [JsonProperty(PropertyName = "file", Required = Required.Always)]
    public RemoteFileDefinition File;
}
public class RemoteFileDefinition
{
    [JsonProperty(PropertyName = "url", Required = Required.Default)]
    public Uri Url;
    [JsonProperty(PropertyName = "sha1", Required = Required.Default)]
    public string SHA1;
    [JsonProperty(PropertyName = "hash", Required = Required.Default)]
    public string Hash;
    [JsonProperty(PropertyName = "path", Required = Required.Default)]
    public string Path;
    [JsonProperty(PropertyName = "size", Required = Required.Default)]
    public int Size;
}
