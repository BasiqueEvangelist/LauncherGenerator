namespace MCApi.Utils;

public static class StringExtensions
{
    //  https://stackoverflow.com/a/298990
    //+ https://stackoverflow.com/a/24829691
    public static IEnumerable<string> SplitCommandLine(string commandLine)
    {
        bool inQuotes = false;
        bool isEscaping = false;

        return commandLine.Split(c =>
            {
                if (c == '\\' && !isEscaping) { isEscaping = true; return false; }

                if (c == '\"' && !isEscaping)
                    inQuotes = !inQuotes;

                isEscaping = false;

                return !inQuotes && char.IsWhiteSpace(c) /*c == ' '*/ ;
            })
            .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
            .Where(arg => !string.IsNullOrEmpty(arg));
    }
    public static IEnumerable<string> Split(this string str,
        Func<char, bool> controller)
    {
        int nextPiece = 0;

        for (int c = 0; c < str.Length; c++)
        {
            if (controller(str[c]))
            {
                yield return str.Substring(nextPiece, c - nextPiece);
                nextPiece = c + 1;
            }
        }

        yield return str.Substring(nextPiece);
    }
    public static string TrimMatchingQuotes(this string input, char quote)
    {
        if (input.Length >= 2 &&
            input[0] == quote && input[input.Length - 1] == quote)
            return input.Substring(1, input.Length - 2);

        return input;
    }
}
