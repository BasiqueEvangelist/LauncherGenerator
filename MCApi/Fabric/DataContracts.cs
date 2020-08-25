using Newtonsoft.Json;

namespace MCApi.Fabric
{
    public class LoaderVersion
    {
        [JsonProperty(PropertyName = "seperator")]
        public string Seperator;
        [JsonProperty(PropertyName = "build")]
        public int Build;
        [JsonProperty(PropertyName = "maven")]
        public string MavenId;
        [JsonProperty(PropertyName = "version")]
        public string Version;
        [JsonProperty(PropertyName = "stable")]
        public bool Stable;
    }
    public class IntermediaryVersion
    {
        [JsonProperty(PropertyName = "maven")]
        public string MavenId;
        [JsonProperty(PropertyName = "version")]
        public string Version;
        [JsonProperty(PropertyName = "stable")]
        public bool Stable;
    }
    public class CompatibleSetupForVersion
    {
        [JsonProperty(PropertyName = "loader")]
        public LoaderVersion Loader;
        [JsonProperty(PropertyName = "intermediary")]
        public IntermediaryVersion Intermediary;
    }
}