using System.Text.RegularExpressions;

namespace FlowSpace.Infrastructure.Common.Extensions;

public static partial class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscore = Regex.Match(input, @"^_+");
        return startUnderscore + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
