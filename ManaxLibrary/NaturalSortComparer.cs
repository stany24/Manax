using System.Text.RegularExpressions;

namespace ManaxLibrary;

public partial class NaturalSortComparer : IComparer<string>
{
    private static readonly Regex Regex = NumbersRegex();
    
    [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
    private static partial Regex NumbersRegex();

    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        List<string> partsX = SplitIntoParts(x);
        List<string> partsY = SplitIntoParts(y);

        int minLength = Math.Min(partsX.Count, partsY.Count);

        for (int i = 0; i < minLength; i++)
        {
            string partX = partsX[i];
            string partY = partsY[i];

            if (long.TryParse(partX, out long numX) && long.TryParse(partY, out long numY))
            {
                int cmp = numX.CompareTo(numY);
                if (cmp != 0) return cmp;
            }
            else
            {
                int cmp = string.Compare(partX, partY, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
            }
        }

        return partsX.Count.CompareTo(partsY.Count);
    }

    private static List<string> SplitIntoParts(string s)
    {
        List<string> parts = [];
        int lastIndex = 0;

        foreach (Match match in Regex.Matches(s))
        {
            if (match.Index > lastIndex) parts.Add(s.Substring(lastIndex, match.Index - lastIndex));
            parts.Add(match.Value);
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < s.Length) parts.Add(s[lastIndex..]);

        if (parts.Count == 0)
            parts.Add(s);

        return parts;
    }
}