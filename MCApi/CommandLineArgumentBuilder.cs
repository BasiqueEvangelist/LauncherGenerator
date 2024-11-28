namespace MCApi;

public class CommandLineArgumentBuilder
{
    List<IGameArgument> args = [];
    public CommandLineArgumentBuilder() { }
    public CommandLineArgumentBuilder Append(string argString)
        => Append(new SimpleArgument(argString));

    public CommandLineArgumentBuilder Append(IGameArgument arg)
    {
        args.Add(arg);
        return this;
    }

    public string Build(IDictionary<string, string> variables, string[] features)
    {
        // allow substituion via simple replace
        var preppedVars = variables.ToDictionary(p => $"${{{p.Key}}}", p => p.Value);
        var strs = BuildImpl(new ListArgument(args), features).Select(s => ProcessArg(s, preppedVars));
        return string.Join(' ', strs);
    }

    static string ProcessArg(string s, IDictionary<string, string> preppedVars)
    {
        if (string.IsNullOrEmpty(s)) return ""; // "\"\"";
        foreach (var p in preppedVars) s = s.Replace(p.Key, p.Value);
        // s = s.Replace("\\", "\\\\");
        s = s.Replace("\"", "\\\"");
        if (s.Any(char.IsWhiteSpace)) s = $"\"{s}\"";

        return s;
    }

    IEnumerable<string> BuildImpl(IGameArgument arg, string[] features)
    {
        if (arg is SimpleArgument sa)
            yield return sa.Value;
        else if (arg is ListArgument la)
        {
            foreach (var childArg in la.Values)
                foreach (var item in BuildImpl(childArg, features))
                    yield return item;
        }
        else if (arg is ComplexArgument ca)
        {
            if (ShouldInclude(ca, features)) 
            {
                foreach (var item in BuildImpl(ca.Value, features))
                    yield return item;
            }
        }
    }

    bool ShouldInclude(ComplexArgument ca, string[] features)
    {
        if (ca.Rules?.Any() == false) return true;

        bool include = false;

        foreach (var rule in ca.Rules!)
        {
            if (!rule.Active(features)) continue;
            include = rule.Action switch
            {
                MCRule.RuleAction.Allow => true,
                MCRule.RuleAction.Disallow => false,
                _ => throw new NotSupportedException()
            };
        }
        return include;
    }

}
