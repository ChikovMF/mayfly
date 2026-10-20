using Microsoft.Extensions.Options;

namespace Server.Host.Files;

internal sealed class RetentionOptionsValidator : IValidateOptions<RetentionOptions>
{
    public ValidateOptionsResult Validate(string? name, RetentionOptions options)
    {
        if (options.MaxLifetime <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail($"{RetentionOptions.SectionName}:{nameof(RetentionOptions.MaxLifetime)} must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
