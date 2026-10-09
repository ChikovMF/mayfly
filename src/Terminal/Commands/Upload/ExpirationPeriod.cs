using System.CommandLine.Parsing;
using System.Globalization;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Upload;

internal static class ExpirationPeriod
{
    public static TimeSpan? Parse(ArgumentResult result)
    {
        var value = result.Tokens.Single().Value;

        var period = TryParse(value);
        if (period is null)
        {
            result.AddError(string.Format(CultureInfo.CurrentCulture, Resources.InvalidExpiresIn, value));
        }

        return period;
    }

    private static TimeSpan? TryParse(string value)
    {
        if (value.Length < 2)
        {
            return null;
        }

        var secondsPerUnit = value[^1] switch
        {
            's' => 1L,
            'm' => 60L,
            'h' => 3600L,
            'd' => 86400L,
            _ => 0L,
        };

        if (secondsPerUnit == 0
            || !int.TryParse(value.AsSpan(..^1), NumberStyles.None, CultureInfo.InvariantCulture, out var amount)
            || amount == 0)
        {
            return null;
        }

        var seconds = amount * secondsPerUnit;
        var maxSeconds = (DateTimeOffset.MaxValue - DateTimeOffset.UtcNow).TotalSeconds;

        return seconds < maxSeconds ? TimeSpan.FromSeconds(seconds) : null;
    }
}
