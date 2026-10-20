using Microsoft.Extensions.Options;

namespace Server.Host.RateLimiting;

internal sealed class PasswordRateLimitOptionsValidator : IValidateOptions<PasswordRateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordRateLimitOptions options)
    {
        var failures = new List<string>();

        if (options.TokenLimit <= 0)
        {
            failures.Add($"{PasswordRateLimitOptions.SectionName}:{nameof(PasswordRateLimitOptions.TokenLimit)} must be greater than zero.");
        }

        if (options.TokensPerPeriod <= 0)
        {
            failures.Add($"{PasswordRateLimitOptions.SectionName}:{nameof(PasswordRateLimitOptions.TokensPerPeriod)} must be greater than zero.");
        }

        if (options.ReplenishmentPeriod <= TimeSpan.Zero)
        {
            failures.Add($"{PasswordRateLimitOptions.SectionName}:{nameof(PasswordRateLimitOptions.ReplenishmentPeriod)} must be greater than zero.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
