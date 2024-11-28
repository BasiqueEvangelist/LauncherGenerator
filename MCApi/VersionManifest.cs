using System.Text.RegularExpressions;
using System.IO.Compression;

namespace MCApi;

public class VersionManifest
{
    protected MCDownload downloader;

    public VersionManifestDefinition DescribedBy { get; }

    internal VersionManifest(VersionManifestDefinition def, MCDownload downloader)
    {
        DescribedBy = def;
        this.downloader = downloader;
    }

    public string ID => DescribedBy.ID;
    public VersionType Type => DescribedBy.Type;
    public Uri Url => DescribedBy.Url;
    public DateTime Time => DescribedBy.Time;
    public DateTime ReleaseTime => DescribedBy.ReleaseTime;
    public int? MinimumLauncherVersion => DescribedBy.MinimumLauncherVersion;
    public string MainClass => DescribedBy.MainClass;
    public Dictionary<string, LoggingSetup> LoggingSettings => DescribedBy.LoggingSettings == null ? new Dictionary<string, LoggingSetup>() : DescribedBy.LoggingSettings.ToDictionary(x => x.Key, x => new LoggingSetup(x.Value));
    public AssetGroup AssetGroup
    {
        get
        {
            if (InheritsFrom != null)
                return getLoadedVersionManifest(InheritsFrom).AssetGroup;
            return new AssetGroup(DescribedBy.AssetGroup, downloader);
        }
    }

    public MCVersion? JarFrom => DescribedBy.JarFrom == null ? InheritsFrom : downloader.GetRemoteVersion(DescribedBy.JarFrom);
    public MCVersion? InheritsFrom => DescribedBy.InheritsFrom == null ? null : downloader.GetRemoteVersion(DescribedBy.InheritsFrom);
    public DescribedRemoteFile Client => getDownload("client");
    public DescribedRemoteFile Server => getDownload("server");

    private DescribedRemoteFile getDownload(string id)
    {
        if (JarFrom != null)
            return getLoadedVersionManifest(JarFrom).getDownload(id);
        return new DescribedRemoteFile(DescribedBy.Downloads[id]);
    }

    public IEnumerable<MCLibrary> Libraries
    {
        get
        {
            if (InheritsFrom != null)
                foreach (MCLibrary lib in getLoadedVersionManifest(InheritsFrom).Libraries)
                {
                    yield return lib;
                }
            foreach (MCLibrary lib in DescribedBy.Libraries.Select(x => new MCLibrary(x)))
            {
                yield return lib;
            }
        }
    }

    public async Task UnpackNatives(string libraryfolder, string nativesfolder)
    {
        Directory.CreateDirectory(nativesfolder);
        await Task.WhenAll(Libraries.Where(x => x.IsNeeded).SelectMany(x => x.NeededDownloads).Where(x => new Regex("natives-(windows|osx|linux)").IsMatch(x.Key)).Select(x => unpackNativesFor(x.Value, libraryfolder, nativesfolder, (str) => { })));
    }
    public async Task UnpackNatives(string libraryfolder, string nativesfolder, Action<string> onunpack)
    {
        Directory.CreateDirectory(nativesfolder);
        await Task.WhenAll(Libraries.Where(x => x.IsNeeded).SelectMany(x => x.NeededDownloads).Where(x => new Regex("natives-(windows|osx|linux)").IsMatch(x.Key)).Select(x => unpackNativesFor(x.Value, libraryfolder, nativesfolder, onunpack)));
    }
    private async Task unpackNativesFor(DescribedRemoteFile f, string lf, string nf, Action<string> onunpack)
    {
        using ZipArchive z = ZipFile.OpenRead(Path.Combine(lf, f.LibraryPath));
        
        foreach (var entry in z.Entries)
        {
            if (entry.FullName.Contains("META-INF")) continue;
                
            using (Stream s = entry.Open())
            using (FileStream fs = File.Open(Path.Combine(nf, entry.FullName), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                await s.CopyToAsync(fs);
                
            onunpack(entry.FullName);
        }
    }

    private IGameArgument getArguments(string id)
    {
        List<IGameArgument> args = [];
        if (InheritsFrom != null)
            args.Add(getLoadedVersionManifest(InheritsFrom).getArguments(id));
        if (DescribedBy.ComplexArguments?.ContainsKey(id) == true)
            args.Add(DescribedBy.ComplexArguments[id]);
        if (args.Count == 0)
            throw new MCDownloadException("Argument type \"" + id + "\" doesn't have any arguments!");
        return new ListArgument(args);
    }

    public IGameArgument MinecraftArguments
    {
        get
        {
            if (DescribedBy.SimpleArguments != null) return ListArgument.FromBuiltString(DescribedBy.SimpleArguments);
            else if (DescribedBy.ComplexArguments != null) return getArguments("game");
            else throw new MCDownloadException("Manifest doesn't have any arguments");
        }
    }
    public IGameArgument JavaArguments
    {
        get
        {
            if (DescribedBy.ComplexArguments != null) return getArguments("jvm");
            else return ListArgument.FromBuiltString("-Djava.library.path=${natives_directory} -Dminecraft.launcher.brand=${launcher_name} -Dminecraft.launcher.version=${launcher_version} -cp ${classpath}");
        }
    }


    // Only call if you know it is loaded (e.g. InheritsFrom or JarFrom)
    private VersionManifest getLoadedVersionManifest(MCVersion v)
    {
        return downloader.getManifestFor(v).GetAwaiter().GetResult();
    }
}
public class LoggingSetup
{
    public LoggingDefinition DescribedBy { get; }

    internal LoggingSetup(LoggingDefinition def)
    {
        DescribedBy = def;
    }

    public string Type => DescribedBy.Type;
    public IGameArgument GameArgument => new SimpleArgument(DescribedBy.GameArgument);
    public DescribedRemoteFile File => new DescribedRemoteFile(DescribedBy.File);
}