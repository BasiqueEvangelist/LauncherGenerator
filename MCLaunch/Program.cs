using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using MCApi;

namespace MCLaunch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!File.Exists("mc.ini"))
            {
                var assembly = typeof(Program).Assembly;
                using (Stream exampleIn = assembly.GetManifestResourceStream("MCLaunch.mc-example.ini"))
                using (FileStream exampleOut = File.OpenWrite("mc.ini"))
                    await exampleIn.CopyToAsync(exampleOut);
            }
            Config cfg = new Config(new FileIniDataParser().ReadFile("mc.ini", Encoding.UTF8));
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
}