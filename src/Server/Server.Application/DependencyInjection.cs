using Microsoft.Extensions.DependencyInjection;
using Server.Application.GetFile;
using Server.Application.GetFileInfo;
using Server.Application.SaveFile;

namespace Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetFileHandler, GetFileHandler>();
        services.AddScoped<IGetFileInfoHandler, GetFileInfoHandler>();
        services.AddScoped<ISaveFileHandler, SaveFileHandler>();

        return services;
    }
}
