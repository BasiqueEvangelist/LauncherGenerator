using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MCApi;

public static partial class VersionManifestExtensions
{
    public static Task DownloadLibraries(this IEnumerable<DescribedRemoteFile> files, string folder, Action<bool, DescribedRemoteFile> onSave)
    {
        return files.DownloadGenericFiles(x => new string[] { Path.Combine(folder, x.LibraryPath) }, onSave);
    }
}
public class MCLibrary
{
    public LibraryDefinition DescribedBy { get; }

    internal MCLibrary(LibraryDefinition def)
    {
        DescribedBy = def;
    }
    #region (All/Needed)Downloads
    public IEnumerable<KeyValuePair<string, DescribedRemoteFile>> AllDownloads
    {
        get
        {
            foreach (var item in unprocessedDownloads())
            {
                if (item.Value.Url.ToString().Contains("maven"))
                    item.Value.Url = new Uri(item.Value.Url + new MavenCoords(Name).LibraryPath);
                yield return item;
            }
        }
    }
    public IEnumerable<KeyValuePair<string, DescribedRemoteFile>> NeededDownloads
    {
        get
        {
            foreach (var item in unprocessedNeededDownloads())
            {
                if (item.Value.Url != null && item.Value.Url.ToString().Contains("maven"))
                    item.Value.Url = new Uri(item.Value.Url + new MavenCoords(Name).LibraryPath);
                yield return item;
            }
        }
    }
    private IEnumerable<KeyValuePair<string, DescribedRemoteFile>> unprocessedDownloads()
    {
        if (DescribedBy.Downloads != null)
        {
            if (DescribedBy.Downloads.ContainsKey("artifact"))
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(DescribedBy.Downloads["artifact"].ToObject<RemoteFileDefinition>()));
            if (DescribedBy.Downloads.ContainsKey("classifiers"))
                foreach (var item in DescribedBy.Downloads["classifiers"].ToObject<Dictionary<string, RemoteFileDefinition>>())
                    yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
        }
        else
        {
            if (DescribedBy.Url != null)
            {
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(DescribedBy.Url.ToString(), new MavenCoords(DescribedBy.Name).LibraryPath));
            }
            else
            {
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile("https://libraries.minecraft.net/" + new MavenCoords(DescribedBy.Name).LibraryPath, new MavenCoords(DescribedBy.Name).LibraryPath));
            }
        }
    }
    private IEnumerable<KeyValuePair<string, DescribedRemoteFile>> unprocessedNeededDownloads()
    {
        if (DescribedBy.Downloads != null)
        {
            if (DescribedBy.Downloads.ContainsKey("artifact"))
            {
                KeyValuePair<string, DescribedRemoteFile> kvp;
                try
                {
                    kvp = KeyValuePair.Create("artifact", new DescribedRemoteFile(DescribedBy.Downloads["artifact"].ToObject<RemoteFileDefinition>())); ;
                }
                catch (JsonSerializationException)
                {
                    yield break; // Forge lol
                }
                yield return kvp;
            }
            if (DescribedBy.Downloads.ContainsKey("classifiers"))
                foreach (var item in DescribedBy.Downloads["classifiers"].ToObject<Dictionary<string, RemoteFileDefinition>>())
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT && item.Key == "natives-windows")
                        yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
                    else if (Environment.OSVersion.Platform == PlatformID.MacOSX && item.Key == "natives-osx")
                        yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
                    else if (Environment.OSVersion.Platform == PlatformID.Unix && item.Key == "natives-linux")
                        yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
                }
        }
        else
        {
            if (DescribedBy.Url != null)
            {
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(DescribedBy.Url.ToString(), new MavenCoords(DescribedBy.Name).LibraryPath));
            }
            else
            {
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile("https://libraries.minecraft.net/" + new MavenCoords(DescribedBy.Name).LibraryPath, new MavenCoords(DescribedBy.Name).LibraryPath));
            }
        }
    }
    #endregion

    public bool IsNeeded
    {
        get
        {
            if (DescribedBy.Rules != null)
            {
                List<MCRule> rulez = DescribedBy.Rules.ToList();
                rulez.Add(new MCRule() { Action = MCRule.RuleAction.Disallow });
                foreach (MCRule rule in rulez)
                {
                    if (rule.OS != null)
                    {
                        if (rule.OS.Name != null)
                        {
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT && rule.OS.Name != "windows")
                                continue;
                            if (Environment.OSVersion.Platform == PlatformID.MacOSX && rule.OS.Name != "osx")
                                continue;
                            if (Environment.OSVersion.Platform == PlatformID.Unix && rule.OS.Name != "linux")
                                continue;
                        }
                        if (rule.OS.VersionRegex != null)
                        {
                            if (!new Regex(rule.OS.VersionRegex).IsMatch(Environment.OSVersion.VersionString))
                                continue;
                        }
                        if (rule.OS.Architecture != null)
                        {
                            if (Environment.Is64BitOperatingSystem && rule.OS.Architecture == "x86")
                                continue;
                        }
                    }
                    if (rule.Action == MCRule.RuleAction.Allow)
                        return true;
                    else if (rule.Action == MCRule.RuleAction.Disallow)
                        return false;
                }
            }
            else
                return true;
            throw new NotImplementedException("WHAT?!?");
        }
    }

    public bool NeedsExtract => DescribedBy.ExtractionSettings != null;
    public bool HasNatives => NeededDownloads.Any(x => new Regex("natives-(windows|osx|linux)").IsMatch(x.Key));

    public string Name => DescribedBy.Name;
    public MCRule[] Rules => DescribedBy.Rules;
}