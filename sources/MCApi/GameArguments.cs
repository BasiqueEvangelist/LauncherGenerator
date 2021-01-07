using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace MCApi
{
    public class GameArguments
    {
        public GameArgument[] Arguments { get; }
        public GameArguments(string s)
        {
            Arguments = StringExtensions.SplitCommandLine(s).Select(x => new GameArgument(x)).ToArray();
        }
        public GameArguments(params GameArguments[] s)
        {
            Arguments = s.SelectMany(x => x.Arguments).ToArray();
        }
        internal GameArguments(IEnumerable<JToken> s)
        {
            Arguments = s.Select(x =>
            {
                if (x.Type == JTokenType.String)
                    return new GameArgument(x.ToObject<string>());
                else
                    return new GameArgument(x.ToObject<ComplexArgument>());
            }).ToArray();
        }
        public string[] Process(IDictionary<string, string> variables, string[] features)
        {
            string[] processed = Arguments.SelectMany(x => x.Process(variables, features)).ToArray();
            return processed.Select(x =>
            {
                string inp = x;
                foreach (var item in variables)
                {
                    inp = inp.Replace("${" + item.Key + "}", item.Value);
                }
                return inp;
            }).ToArray();
        }
        public string ProcessFlat(IDictionary<string, string> variables, string[] features) => FoldArgs(Process(variables, features));
        public static string FoldArgs(IEnumerable<string> s)
        {
            List<string> formatted = new List<string>();
            foreach (string st in s)
            {
                string progr = st;
                //progr = progr.Replace("\\", "\\\\");
                progr = progr.Replace("\"", "\\\"");
                if (progr.Any(x => char.IsWhiteSpace(x)) || String.IsNullOrWhiteSpace(progr))
                    progr = "\"" + progr + "\"";
                formatted.Add(progr);
            }
            return string.Join(' ', formatted);
        }

        public static GameArguments operator +(GameArguments _1, GameArguments _2)
        {
            return new GameArguments(_1, _2);
        }
    }
    public struct GameArgument
    {
        public GameArgument(ComplexArgument arg)
        {
            if (arg.Value.Type == JTokenType.String)
            {
                simpleValue = arg.Value.ToObject<string>();
                complexValue = null;
            }
            else
            {
                complexValue = new GameArguments(arg.Value.ToObject<JToken[]>());
                simpleValue = null;
            }
            Rules = arg.Rules == null ? new MCRule[0] : arg.Rules;
        }
        public GameArgument(string s)
        {
            Rules = new MCRule[0];
            complexValue = null;
            simpleValue = s;
        }
        private string? simpleValue;
        private GameArguments? complexValue;
        public MCRule[] Rules;
        public string[] Process(IDictionary<string, string> variables, string[] features)
        {
            if (!IsRequired(features))
                return Array.Empty<string>();
            if (complexValue != null) return complexValue.Process(variables, features);
            else if (simpleValue != null) return new string[] { simpleValue };
            else throw new NotImplementedException();
        }
        public bool IsRequired(string[] features)
        {
            if (Rules == null)
                return true;
            if (Rules.Length == 0)
                return true;
            else
            {
                List<MCRule> rulez = Rules.ToList();
                rulez.Append(new MCRule() { Action = MCRule.RuleAction.disallow });
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
                    if (rule.RequiredFeatures != null)
                    {
                        bool cont = false;
                        foreach (var pair in rule.RequiredFeatures)
                        {
                            if (pair.Value)
                            {
                                if (!features.Contains(pair.Key))
                                {
                                    cont = true;
                                    break;
                                }
                            }
                            else
                            if (features.Contains(pair.Key))
                            {
                                cont = true;
                                break;
                            }
                        }
                        if (cont) continue;
                    }
                    if (rule.Action == MCRule.RuleAction.allow)
                        return true;
                    else if (rule.Action == MCRule.RuleAction.disallow)
                        return false;
                }
            }
            return false;
        }
    }
}