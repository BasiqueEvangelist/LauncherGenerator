using Nuke.Common;
using Nuke.Common.Tooling;

namespace LauncherGenerator.Build
{
    public static partial class CargoTasks
    {
        private static void CustomLogger(OutputType outputType, string text)
        {
            Logger.Normal(text);
        }
    }
}