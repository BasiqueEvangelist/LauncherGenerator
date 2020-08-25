using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace MCApi
{
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
        public int MinimumLauncherVersion => DescribedBy.MinimumLauncherVersion;
        public string MainClass => DescribedBy.MainClass;
        public Dictionary<string, LoggingSetup> LoggingSettings => DescribedBy.LoggingSettings == null ? new Dictionary<string, LoggingSetup>() : DescribedBy.LoggingSettings.ToDictionary(x => x.Key, x => new LoggingSetup(x.Value));
        public AssetGroup AssetGroup
        {
            get
            {
                if (DescribedBy.InheritsFrom != null)
                    return getLoadedVersionManifest(InheritsFrom).AssetGroup;
                return new AssetGroup(DescribedBy.AssetGroup, downloader);
            }
        }

        public MCVersion JarFrom => DescribedBy.JarFrom == null ? (InheritsFrom ?? downloader.GetRemoteVersion(ID)) : downloader.GetRemoteVersion(DescribedBy.JarFrom);
        public MCVersion InheritsFrom => DescribedBy.InheritsFrom == null ? null : downloader.GetRemoteVersion(DescribedBy.InheritsFrom);
        public DescribedRemoteFile Client => getDownload("client");
        public DescribedRemoteFile Server => getDownload("server");

        private DescribedRemoteFile getDownload(string id)
        {
            if (DescribedBy.JarFrom != null || DescribedBy.InheritsFrom != null)
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
            using (FileStream zfs = File.OpenRead(Path.Combine(lf, f.LibraryPath)))
            using (ZipFile z = new ZipFile(zfs))
            {
                await Task.WhenAll(new EnumerableFixer<ZipEntry>(z).Where(x => !x.Name.Contains("META-INF")).Select(async x =>
                {
                    using (Stream s = z.GetInputStream(x))
                    using (FileStream fs = File.Open(Path.Combine(nf, x.Name), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        await s.CopyToAsync(fs);
                    onunpack(x.Name);
                }));
            }
        }

        private GameArguments getArguments(string id)
        {
            GameArguments args = new GameArguments();
            if (InheritsFrom != null)
                args += getLoadedVersionManifest(InheritsFrom).getArguments(id);
            if (DescribedBy.ComplexArguments.ContainsKey(id))
                args += new GameArguments(DescribedBy.ComplexArguments[id]);
            if (args.Arguments.Length == 0)
                throw new MCDownloadException("Argument type \"" + id + "\" doesn't have any arguments!");
            return args;
        }

        public GameArguments MinecraftArguments
        {
            get
            {
                if (DescribedBy.SimpleArguments != null) return new GameArguments(DescribedBy.SimpleArguments);
                else if (DescribedBy.ComplexArguments != null) return getArguments("game");
                else throw new MCDownloadException("Manifest doesn't have any arguments");
            }
        }
        public GameArguments JavaArguments
        {
            get
            {
                if (DescribedBy.ComplexArguments != null) return getArguments("jvm");
                else return new GameArguments("-Djava.library.path=${natives_directory} -Dminecraft.launcher.brand=${launcher_name} -Dminecraft.launcher.version=${launcher_version} -cp ${classpath}");
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
        public GameArguments GameArgument => new GameArguments(DescribedBy.GameArgument);
        public DescribedRemoteFile File => new DescribedRemoteFile(DescribedBy.File);
    }
}