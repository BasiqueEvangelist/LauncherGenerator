using System.Net.Http;
using System.Security.Cryptography;

namespace MCApi;

public static partial class VersionManifestExtensions
{
    /// Generic function for downloading any RemoteFile.
    public static Task DownloadGenericFiles<TFile>(this IEnumerable<TFile> files, Func<TFile, string[]> pathFunc, Action<bool, TFile>? onSave = null) where TFile : RemoteFile
    {
        return Task.WhenAll(files.Select(async x =>
        {
            var created = await x.Save(pathFunc(x));
            if (onSave != null)
                onSave(created, x);
        }));
    }
}

public interface RemoteFile
{
    public const bool ASSUME_CORRECT = true;

    Uri Url { get; }
    bool HasHash { get; }
    string Hash { get; }
    int Size { get; }

    public async Task<bool> Save(string[] to)
    {
        if (to.Length < 1)
            return false;
        foreach (string path in to)
        {
            if (Path.GetDirectoryName(path) != "")
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path))
            {
                if (ASSUME_CORRECT)
                {
#pragma warning disable CS0162
                    bool created = false;
                    foreach (string copypath in to)
                        if (!File.Exists(copypath))
                        {
                            File.Copy(path, copypath);
                            created = true;
                        }
                    return created;
                }
                else
                {
                    if (!HasHash)
#pragma warning restore CS0162
                        File.Delete(path);
                    else
                    {
                        string fh;
                        using (SHA1Managed s1 = new SHA1Managed())
                        using (FileStream s = File.OpenRead(path))
                            fh = string.Concat(s1.ComputeHash(s).Select(x => x.ToString("x2")));
                        if (fh == Hash)
                        {
                            bool created = false;
                            foreach (string copypath in to)
                                if (!File.Exists(copypath))
                                {
                                    File.Copy(path, copypath);
                                    created = true;
                                }
                            return created;
                        }
                        else
                            File.Delete(path);
                    }
                }
            }
        }
        using (var nswrap = await MCHttpHelper.Open(Url))
        using (var ns = nswrap.Value)
        using (FileStream fs = File.Open(to[0], FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await ns.CopyToAsync(fs);
        foreach (string copypath in to)
            if (!File.Exists(copypath))
                File.Copy(to[0], copypath);
        return true;
    }

    public string Name
    {
        get => Path.GetFileName(Url.AbsolutePath);
    }
}

public class DescribedRemoteFile : RemoteFile
{
    public const bool ASSUME_CORRECT = true;
    public RemoteFileDefinition DescribedBy { get; }
    private int size = -1;

    public DescribedRemoteFile(RemoteFileDefinition def)
    {
        DescribedBy = def;
        if (def.Size != default(int))
        {
            size = def.Size;
        }
        else
        {
            Task.Run(async () =>
            {
                HttpResponseMessage msg = await MCHttpHelper.Head(Url);
                if (msg.Headers.Contains("Content-Size"))
                    size = int.Parse(msg.Headers.GetValues("Content-Size").First());
            });
        }
    }

    public DescribedRemoteFile(string url, string libraryPath) : this(new RemoteFileDefinition()
    {
        Url = new Uri(url),
        Path = libraryPath
    })
    {

    }

    public Uri Url
    {
        get
        {
            return DescribedBy.Url;
        }
        set
        {
            DescribedBy.Url = value;
        }
    }

    public string Hash
    {
        get
        {
            if (DescribedBy.SHA1 != null)
                return DescribedBy.SHA1;
            else if (DescribedBy.Hash != null)
                return DescribedBy.Hash;
            else
                throw new KeyNotFoundException();
        }
    }
    public bool HasHash => DescribedBy.SHA1 != null || DescribedBy.Hash != null;
    public int Size
    {
        get
        {

            return size;
        }
    }
    public string LibraryPath
    {
        get
        {
            if (DescribedBy.Path == null)
                throw new KeyNotFoundException();
            else
                return DescribedBy.Path;
        }
    }

    public static bool operator ==(DescribedRemoteFile _1, DescribedRemoteFile _2)
    {
        return _1.Url == _2.Url;
    }
    public static bool operator !=(DescribedRemoteFile _1, DescribedRemoteFile _2)
    {
        return _1.Url != _2.Url;
    }
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(DescribedRemoteFile)) return false;
        return ((DescribedRemoteFile)obj).Url == Url;
    }
    public override int GetHashCode()
    {
        return Url.GetHashCode();
    }
}