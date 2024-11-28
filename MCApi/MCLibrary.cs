using System.Text.RegularExpressions;

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
        if (DescribedBy.Downloads is { } downloads)
        {
            if (downloads.Artifact is { } artifact)
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(artifact));
            if (downloads.Classifiers is { } classifiers)
                foreach (var item in classifiers)
                    yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
        }
        else
        {
            var mavenCoords = new MavenCoords(DescribedBy.Name);
            var url = DescribedBy.Url?.ToString() ?? $"https://libraries.minecraft.net/{mavenCoords.LibraryPath}";
            yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(url, mavenCoords.LibraryPath));
        }
    }
    private IEnumerable<KeyValuePair<string, DescribedRemoteFile>> unprocessedNeededDownloads()
    {
        if (DescribedBy.Downloads is { } downloads)
        {
            if (downloads.Artifact is { } artifact)
                yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(artifact));
            if (downloads.Classifiers is { } classifiers)
            {
                foreach (var item in classifiers)
                {
                    if (SystemInfo.CurrentNativesPlatform == item.Key)
                        yield return KeyValuePair.Create(item.Key, new DescribedRemoteFile(item.Value));
                }
            }
        }
        else
        {
            var mavenCoords = new MavenCoords(DescribedBy.Name);
            var url = DescribedBy.Url?.ToString() ?? $"https://libraries.minecraft.net/{mavenCoords.LibraryPath}";
            yield return KeyValuePair.Create("artifact", new DescribedRemoteFile(url, mavenCoords.LibraryPath));
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
                var allow = false;
                foreach (MCRule rule in DescribedBy.Rules)
                {
                    if (!rule.Active()) continue;
                    if (rule.Action == MCRule.RuleAction.Allow)
                        allow = true;
                    else if (rule.Action == MCRule.RuleAction.Disallow)
                        allow = false;
                }
                return allow;
            }
            else
                return true;
        }
    }

    public bool NeedsExtract => DescribedBy.ExtractionSettings != null;
    public bool HasNatives => NeededDownloads.Any(x => new Regex("natives-(windows|osx|linux)").IsMatch(x.Key));

    public string Name => DescribedBy.Name;
    public MCRule[] Rules => DescribedBy.Rules;
}