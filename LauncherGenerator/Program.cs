using IniParser;
using LauncherGenerator.Utils;

namespace LauncherGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        var configPath = "mc.ini";
        if (!File.Exists(configPath))
        {
            await Resources.ExtractEmbeddedFile("mc-example.ini", configPath);
        }
        Config cfg = new Config(new FileIniDataParser().ReadFile(configPath, Encoding.UTF8));
        if (cfg.IsStub)
        {
            Log.Error("Please edit config");
            return;
        }
        Log.Step("Loaded config");

        Directory.CreateDirectory("data");
        Directory.CreateDirectory("data/libraries");
        Directory.CreateDirectory("data/versions");
        Directory.CreateDirectory("data/assets/indexes");
        Directory.CreateDirectory("data/assets/objects");
        Directory.CreateDirectory("data/assets/log_configs");
        Directory.CreateDirectory("data/assets/virtual");

        if (args.Length < 1)
        {
            await ProgramTasks.Build(cfg);
            return;
        }
        switch (args[0])
        {
            case "build":
                await ProgramTasks.Build(cfg);
                return;
            case "gc":
                await ProgramTasks.CollectGarbage(cfg);
                return;
            default:
                Log.Error($"No such subcommand {args[0]}");
                return;
        }
    }
}