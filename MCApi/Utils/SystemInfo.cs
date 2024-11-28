namespace MCApi;

static internal class SystemInfo
{
    public static string CurrentPlatform { get; } = CurrentPlatformImpl();
    static string CurrentPlatformImpl()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        throw new NotSupportedException("Unknown platform");
    }
    public static string CurrentNativesPlatform { get; } = CurrentNativesPlatformImpl();
    static string CurrentNativesPlatformImpl()
    {
        if (OperatingSystem.IsWindows()) return "natives-windows";
        if (OperatingSystem.IsLinux()) return "natives-linux";
        if (OperatingSystem.IsMacOS()) return "natives-osx";
        throw new NotSupportedException("Unknown platform");
    }

}
