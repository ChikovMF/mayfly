using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage;

internal sealed class YandexDiskOptionsValidator : IValidateOptions<YandexDiskOptions>
{
    private static readonly TimeSpan MaxTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

    public ValidateOptionsResult Validate(string? name, YandexDiskOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.OAuthToken))
        {
            failures.Add($"{YandexDiskOptions.SectionName}:{nameof(YandexDiskOptions.OAuthToken)} must be set.");
        }

        if (options.Timeout <= TimeSpan.Zero || options.Timeout > MaxTimeout)
        {
            failures.Add($"{YandexDiskOptions.SectionName}:{nameof(YandexDiskOptions.Timeout)} must be greater than zero and at most {MaxTimeout}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
