namespace MCApi;

public static partial class VersionManifestExtensions
{
    public static Task DownloadObjects(this IEnumerable<AssetIndexObject> objects, string folder, Action<bool, AssetIndexObject>? onload = null)
    {
        return objects.DownloadGenericFiles(x => new string[] { Path.Combine(folder, getParts(x.Hash)) }, onload);
    }

    public static Task DownloadObjectsFlat(this IEnumerable<AssetIndexObject> objects, string folder, Action<bool, AssetIndexObject>? onload = null)
    {
        return objects.DownloadGenericFiles(x => x.Paths.Select(y => Path.Combine(folder, y)).ToArray(), onload);
    }

    private static string getParts(string hash)
    {
        return hash.Substring(0, 2) + "/" + hash;
    }

}
public class AssetGroup
{
    protected MCDownload downloader;

    public AssetGroupDefinition DescribedBy { get; }

    internal AssetGroup(AssetGroupDefinition def, MCDownload downloader)
    {
        DescribedBy = def;
        this.downloader = downloader;
    }

    public Task<AssetGroupIndex> GetIndex() => downloader.getAIndexFor(this);

    public string ID => DescribedBy.ID;
    public string Hash => DescribedBy.Hash;
    public int TotalSize => DescribedBy.TotalSize;
    public int Size => DescribedBy.TotalSize;
}
public class AssetGroupIndex
{
    public AssetGroupIndexDefinition DescribedBy { get; }
    internal AssetGroupIndex(AssetGroupIndexDefinition def)
    {
        DescribedBy = def;
    }
    public bool IsVirtual => DescribedBy.IsVirtual;
    public IEnumerable<AssetIndexObject> Objects => DescribedBy.Objects.GroupBy(x => x.Value.Hash).Select(x => new AssetIndexObject(x.Key, x));

    public async Task UnpackVirtuals(string objectfolder, string virtualfolder)
    {
        if (!IsVirtual)
            return;
        await Task.WhenAll(Objects.Select(x => unpackVirtFor(objectfolder, virtualfolder, x)));
    }

    private async Task unpackVirtFor(string objectfolder, string virtualfolder, AssetIndexObject asset)
    {
        foreach (string path in asset.Paths)
        {
            if (File.Exists(Path.Combine(virtualfolder, path)))
            {
                File.Delete(Path.Combine(virtualfolder, path));
            }
            await Task.Run(() =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(virtualfolder, path)));
                File.Copy(Path.Combine(objectfolder, getParts(asset.Hash)), Path.Combine(virtualfolder, path));
            });
        }
    }
    private static string getParts(string hash)
    {
        return hash.Substring(0, 2) + "/" + hash;
    }
}
public class AssetIndexObject : RemoteFile
{
    private readonly string hash;
    private readonly IEnumerable<KeyValuePair<string, AssetInfo>> vals;

    internal AssetIndexObject(string hash, IEnumerable<KeyValuePair<string, AssetInfo>> vals)
    {
        this.hash = hash;
        this.vals = vals;
    }
    public string Hash => hash;
    public int Size => vals.First().Value.Size;
    public string[] Paths => vals.Select(x => x.Key).ToArray();

    public Uri Url => new Uri("https://resources.download.minecraft.net/" + getParts(Hash));

    public bool HasHash => true;

    private static string getParts(string hash)
    {
        return hash.Substring(0, 2) + "/" + hash;
    }
}