namespace Server.Host.RateLimiting;

internal static class PasswordRateLimiting
{
    public static IServiceCollection AddPasswordRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton<PasswordRateLimiterPolicy>();
        services.AddRateLimiter(limiter => limiter.AddPolicy<string, PasswordRateLimiterPolicy>(PasswordRateLimiterPolicy.Name));

        return services;
    }
}
