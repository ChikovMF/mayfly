using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage.S3;

internal sealed class S3OptionsValidator : IValidateOptions<S3Options>
{
    public ValidateOptionsResult Validate(string? name, S3Options options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out var serviceUrl)
            || (serviceUrl.Scheme != Uri.UriSchemeHttp && serviceUrl.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add($"{S3Options.SectionName}:{nameof(S3Options.ServiceUrl)} must be an absolute http or https URL.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
        {
            failures.Add($"{S3Options.SectionName}:{nameof(S3Options.AccessKey)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            failures.Add($"{S3Options.SectionName}:{nameof(S3Options.SecretKey)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            failures.Add($"{S3Options.SectionName}:{nameof(S3Options.BucketName)} must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            failures.Add($"{S3Options.SectionName}:{nameof(S3Options.Region)} must be set.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
