using System.Collections.Generic;
using System.Linq;
using IniParser.Model;

namespace MCLaunch
{
    public class Config
    {
        public string Username;
        public bool IsStub = false;
        public Target[] Targets;
        public Config(IniData ini)
        {
            Username = ini["mcauth"]["username"];
            if (ini.Global.ContainsKey("stub"))
                IsStub = ini.Global["stub"] != "no";
            Targets = ini.Sections.Where(x => x.SectionName != "mcauth").Select(x => new Target(x)).ToArray();
        }
    }
    public struct Target
    {
        public string Name;
        public string VersionID;
        public string Profile;
        public string JVMArguments;
        public string JavaPath;
        public string NewGameArguments;
        public string Transformer;

        public Target(SectionData x)
        {
            Name = x.SectionName;
            VersionID = x.Keys["version"];
            Profile = x.Keys["profile"];
            JVMArguments = x.Keys["jvmargs"] ?? "";
            JavaPath = x.Keys["javapath"] ?? "java";
            NewGameArguments = x.Keys["gameargs"] ?? "";
            Transformer = x.Keys["transformer"] ?? "none";
        }
    }
}
