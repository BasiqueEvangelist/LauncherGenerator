namespace LauncherGenerator;

public static class GCLogic
{
    public static async Task CollectAssets(VersionManifest[] manifests)
    {
        var allAssetIndexes = await Task.WhenAll(
            manifests.Select(x => x.AssetGroup.GetIndex())
        );
        var allAssets = allAssetIndexes.SelectMany(x => x.Objects).Select(x => x.Hash).ToArray();

        foreach (string pathFirst in Directory.EnumerateDirectories("data/assets/objects"))
        {
            foreach (string path in Directory.EnumerateFiles(pathFirst))
            {
                string hash = path.Substring("data/assets/objects/aa/".Length);
                if (!allAssets.Contains(hash))
                {
                    File.Delete(path);
                    Log.FileRm(path);
                }
            }
        }
    }

    public static void CollectScripts(Target[] targets)
    {
        foreach (string path in Directory.EnumerateFiles("data"))
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            
            if (ext == ".bat" || ext == ".sh")
                if (!targets.Any(x => x.Name == filename))
                {
                    File.Delete(path);
                    Log.FileRm(path);
                }
        }
    }

    public static void CollectLibraries(VersionManifest[] manifests)
    {
        DescribedRemoteFile[] libsDownloaded = manifests
            .SelectMany(x => x.Libraries)
            .SelectMany(x => x.NeededDownloads)
            .Select(x => x.Value)
            .Distinct()
            .ToArray();
        
        foreach (string path in WalkDirectory("data/libraries"))
        {
            if (!libsDownloaded.Any(x => Path.Combine("data", "libraries", x.LibraryPath) == path))
            {
                File.Delete(path);
                Log.FileRm(path);
            }
        }
    }

    private static List<string> WalkDirectory(string path)
    {
        var l = new List<string>();
        foreach (var e in Directory.EnumerateFileSystemEntries(path))
        {
            if (File.GetAttributes(e).HasFlag(FileAttributes.Directory))
                l.AddRange(WalkDirectory(e));
            else
                l.Add(e);
        }
        return l;
    }
}