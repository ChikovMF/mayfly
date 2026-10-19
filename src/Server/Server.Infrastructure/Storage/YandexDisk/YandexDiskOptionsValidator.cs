using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage.YandexDisk;

internal sealed class YandexDiskOptionsValidator : IValidateOptions<YandexDiskOptions>
{
    public ValidateOptionsResult Validate(string? name, YandexDiskOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.OAuthToken))
        {
            failures.Add($"{YandexDiskOptions.SectionName}:{nameof(YandexDiskOptions.OAuthToken)} must be set.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
