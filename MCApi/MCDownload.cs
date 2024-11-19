using System.Collections.Concurrent;

namespace MCApi;

public class MCDownload
{
    internal static ConcurrentDictionary<string, Lazy<Task<VersionManifest>>> manifestCache = new ConcurrentDictionary<string, Lazy<Task<VersionManifest>>>();
    internal ConcurrentDictionary<string, Lazy<Task<AssetGroupIndex>>> aIndexCache = new ConcurrentDictionary<string, Lazy<Task<AssetGroupIndex>>>();

    private MCVersion[]? versions;
    private IVersionResolver resolver;

    public MCDownload(IVersionResolver resolver)
    {
        this.resolver = resolver;
    }


    #region getXFor loadX
    internal Task<VersionManifest> getManifestFor(MCVersion v)
    {
        return manifestCache.GetOrAdd(v.ID, new Lazy<Task<VersionManifest>>(() => loadManif(v), true)).Value;
    }
    internal Task<AssetGroupIndex> getAIndexFor(AssetGroup a)
    {
        return aIndexCache.GetOrAdd(a.ID, new Lazy<Task<AssetGroupIndex>>(() => loadAIndex(a), true)).Value;
    }
    private async Task<VersionManifest> loadManif(MCVersion v)
    {
        VersionManifestDefinition def = await resolver.GetVersion(v);
        if (def.JarFrom != null)
            await getManifestFor(GetRemoteVersion(def.JarFrom));
        if (def.InheritsFrom != null)
            await getManifestFor(GetRemoteVersion(def.InheritsFrom));
        return new VersionManifest(def, this);
    }

    private async Task<AssetGroupIndex> loadAIndex(AssetGroup a)
    {
        AssetGroupIndexDefinition def = await resolver.GetAssetIndex(a);
        return new AssetGroupIndex(def);
    }
    #endregion
    #region Interface
    public async Task Init()
    {
        if (versions != null) return;
        versions = (await resolver.GetAllVersions()).Select(x => new MCVersion(x, this)).ToArray();
    }

    public MCVersion[] RemoteVersions()
    {
        if (versions == null) throw new MCDownloadException("MCDownload isn't initialized!");
        return versions;
    }

    public MCVersion GetRemoteVersion(string vid)
    {
        foreach (MCVersion v in RemoteVersions())
        {
            if (v.ID == vid)
                return v;
        }
        throw new MCDownloadException("Could not find version \"" + vid + "\"");
    }
    #endregion
}

public class MCVersion
{
    protected MCDownload downloader;

    public VersionDefinition DescribedBy { get; }
    internal MCVersion(VersionDefinition def, MCDownload downloader)
    {
        DescribedBy = def;
        this.downloader = downloader;
    }
    public string ID => DescribedBy.ID;
    public VersionType Type => DescribedBy.Type;
    public Uri Url => DescribedBy.Url;
    public Task<VersionManifest> GetManifest() => downloader.getManifestFor(this);
    public DateTime Time => DescribedBy.Time;
    public DateTime ReleaseTime => DescribedBy.ReleaseTime;


    public override bool Equals(object obj)
    {
        if (obj is MCVersion other)
            return ID == other.ID;
        return false;
    }

    public override int GetHashCode() => DescribedBy.ID.GetHashCode();
}
