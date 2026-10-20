using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Configuration;

public static class SectionOptionsExtensions
{
    /// <summary>Adds <typeparamref name="TOptions"/> bound to its configuration section.</summary>
    public static OptionsBuilder<TOptions> AddOptions<TOptions>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class, ISectionOptions =>
        services.AddOptions<TOptions>().Bind(configuration.GetSection(TOptions.SectionName));

    /// <summary>Adds <typeparamref name="TOptions"/> bound to its configuration section and checked by <typeparamref name="TValidator"/>.</summary>
    public static OptionsBuilder<TOptions> AddOptions<TOptions, TValidator>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class, ISectionOptions
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        return services.AddOptions<TOptions>(configuration).ValidateOnStart();
    }
}
