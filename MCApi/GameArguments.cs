using System.Data;
using System.Text.RegularExpressions;

namespace MCApi;

public class CommandLineArgumentBuilder
{
    List<IGameArgument> args = [];
    public CommandLineArgumentBuilder() { }
    public CommandLineArgumentBuilder Append(IGameArgument arg)
    {
        args.Add(arg);
        return this;
    }

    public IEnumerable<string> Build(IDictionary<string, string> variables, string[] features) 
        => BuildOne(new ListArgument(args), variables, features);

    IEnumerable<string> BuildOne(IGameArgument arg, IDictionary<string, string> variables, string[] features)
    {
        if (arg is SimpleArgument sa)
            yield return sa.Value;
        else if (arg is ListArgument la)
        {
            foreach (var childArg in la.Values)
                foreach (var item in BuildOne(childArg, variables, features))
                    yield return item;
        }
        else if (arg is ComplexArgument ca)
        {
            if (ShouldInclude(ca, features)) 
            {
                foreach (var item in BuildOne(ca.Value, variables, features))
                    yield return item;
            }
        }
    }

    bool ShouldInclude(ComplexArgument ca, string[] features)
    {
        if (ca.Rules?.Any() == false)
            return true;

        bool include = false;

        foreach (var rule in ca.Rules!)
        {
            if (!RuleApplies(rule, features))
                continue;

            if (rule.Action == MCRule.RuleAction.Allow)
                include = true;
            else if (rule.Action == MCRule.RuleAction.Disallow)
                include = false;
        }
        return include;
    }

    static bool RuleApplies(MCRule rule, string[] features)
    {
        if (rule.OS != null)
        {
            if (rule.OS.Name is string name )
            {
                if (name != CurrentPlatform)
                    return false;
            }
            if (rule.OS.VersionRegex != null)
            {
                if (!new Regex(rule.OS.VersionRegex).IsMatch(Environment.OSVersion.VersionString))
                    return false;
            }
            if (rule.OS.Architecture != null)
            {
                if (Environment.Is64BitOperatingSystem && rule.OS.Architecture == "x86")
                    return false;
            }
        }

        if (rule.RequiredFeatures != null)
        {
            foreach (var pair in rule.RequiredFeatures)
            {
                if (pair.Value != features.Contains(pair.Key))
                    return false;
            }
        }
        return true;
    }

    static string CurrentPlatform { get; } = CurrentPlatformImpl();
    static string CurrentPlatformImpl()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        throw new NotSupportedException("Unknown platform");
    }
}

//public class GameArgumentssssssss
//{
//    public GameArgument[] Arguments { get; }
//    public GameArguments(string s)
//    {
//        Arguments = StringExtensions.SplitCommandLine(s).Select(x => new GameArgument(x)).ToArray();
//    }
//    public GameArguments(params GameArguments[] s)
//    {
//        Arguments = s.SelectMany(x => x.Arguments).ToArray();
//    }
//    internal GameArguments(IEnumerable<JToken> s)
//    {
//        Arguments = s.Select(x =>
//        {
//            if (x.Type == JTokenType.String)
//                return new GameArgument(x.ToObject<string>());
//            else
//                return new GameArgument(x.ToObject<ComplexArgument>());
//        }).ToArray();
//    }
//    public string[] Process(IDictionary<string, string> variables, string[] features)
//    {
//        string[] processed = Arguments.SelectMany(x => x.Process(variables, features)).ToArray();
//        return processed.Select(x =>
//        {
//            string inp = x;
//            foreach (var item in variables)
//            {
//                inp = inp.Replace("${" + item.Key + "}", item.Value);
//            }
//            return inp;
//        }).ToArray();
//    }
//    public string ProcessFlat(IDictionary<string, string> variables, string[] features) => FoldArgs(Process(variables, features));
//    public static string FoldArgs(IEnumerable<string> s)
//    {
//        List<string> formatted = new List<string>();
//        foreach (string st in s)
//        {
//            string progr = st;
//            //progr = progr.Replace("\\", "\\\\");
//            progr = progr.Replace("\"", "\\\"");
//            if (progr.Any(x => char.IsWhiteSpace(x)) || String.IsNullOrWhiteSpace(progr))
//                progr = "\"" + progr + "\"";
//            formatted.Add(progr);
//        }
//        return string.Join(' ', formatted);
//    }

//    public static GameArguments operator +(GameArguments _1, GameArguments _2)
//    {
//        return new GameArguments(_1, _2);
//    }
//}
//public struct GameArgumentttttttttt
//{
//    public GameArgument(ComplexArgument arg)
//    {
//        if (arg.Value.Type == JTokenType.String)
//        {
//            simpleValue = arg.Value.ToObject<string>();
//            complexValue = null;
//        }
//        else
//        {
//            complexValue = new GameArguments(arg.Value.ToObject<JToken[]>());
//            simpleValue = null;
//        }
//        Rules = arg.Rules == null ? new MCRule[0] : arg.Rules;
//    }
//    public GameArgument(string s)
//    {
//        Rules = new MCRule[0];
//        complexValue = null;
//        simpleValue = s;
//    }
//    private string? simpleValue;
//    private GameArguments? complexValue;
//    public MCRule[] Rules;
//    public string[] Process(IDictionary<string, string> variables, string[] features)
//    {
//        if (!IsRequired(features))
//            return Array.Empty<string>();
//        if (complexValue != null) return complexValue.Process(variables, features);
//        else if (simpleValue != null) return new string[] { simpleValue };
//        else throw new NotImplementedException();
//    }
//}