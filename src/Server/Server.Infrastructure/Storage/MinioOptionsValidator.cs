using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage;

internal sealed class MinioOptionsValidator : IValidateOptions<MinioOptions>
{
    public ValidateOptionsResult Validate(string? name, MinioOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            failures.Add($"{MinioOptions.SectionName}:{nameof(MinioOptions.Endpoint)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
        {
            failures.Add($"{MinioOptions.SectionName}:{nameof(MinioOptions.AccessKey)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            failures.Add($"{MinioOptions.SectionName}:{nameof(MinioOptions.SecretKey)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            failures.Add($"{MinioOptions.SectionName}:{nameof(MinioOptions.BucketName)} must be set.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
