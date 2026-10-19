using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Server.Application;
using Server.Domain.Abstractions;
using Server.Host;
using Server.Host.Cleanup;
using Server.Host.Endpoints;
using Server.Host.ErrorHandling;
using Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var upload = builder.Configuration.GetSection(UploadOptions.SectionName).Get<UploadOptions>() ?? new UploadOptions();
builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = upload.MaxRequestBodySize);
builder.Services.Configure<FormOptions>(form => form.MultipartBodyLengthLimit = upload.MaxRequestBodySize);

builder.Services.AddOptions<CleanupOptions>()
    .Bind(builder.Configuration.GetSection(CleanupOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<CleanupOptions>, CleanupOptionsValidator>();
builder.Services.AddHostedService<ExpiredFilesCleanupService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

var app = builder.Build();

app.Services.GetRequiredService<IDatabaseMigrator>().Migrate();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapFilesEndpoints();

app.Run();
