using Microsoft.Extensions.Options;

namespace Server.Host.Cleanup;

internal sealed class CleanupOptionsValidator : IValidateOptions<CleanupOptions>
{
    private static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    public ValidateOptionsResult Validate(string? name, CleanupOptions options)
    {
        var failures = new List<string>();

        if (options.Interval <= TimeSpan.Zero || options.Interval > MaxInterval)
        {
            failures.Add($"{CleanupOptions.SectionName}:{nameof(CleanupOptions.Interval)} must be greater than zero and at most {MaxInterval}.");
        }

        if (options.Lease <= TimeSpan.Zero)
        {
            failures.Add($"{CleanupOptions.SectionName}:{nameof(CleanupOptions.Lease)} must be greater than zero.");
        }

        if (options.MaxAttempts <= 0)
        {
            failures.Add($"{CleanupOptions.SectionName}:{nameof(CleanupOptions.MaxAttempts)} must be greater than zero.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
