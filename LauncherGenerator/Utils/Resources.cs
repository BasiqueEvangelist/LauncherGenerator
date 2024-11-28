namespace LauncherGenerator.Utils;

static internal class Resources
{
    public static async Task ExtractEmbeddedFile(string name, string path)
    {
        var assembly = typeof(Program).Assembly;
        using (var jarIn = assembly.GetManifestResourceStream($"{nameof(LauncherGenerator)}.{name}") ?? throw new NotImplementedException())
        using (var jarOut = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Delete))
            await jarIn.CopyToAsync(jarOut);
    }
}
