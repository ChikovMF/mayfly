using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Server.Application;
using Server.Domain.Abstractions;
using Server.Host.Cleanup;
using Server.Host.ErrorHandling;
using Server.Host.Files;
using Server.Host.Health;
using Server.Host.RateLimiting;
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

builder.Services.AddOptions<PasswordRateLimitOptions>()
    .Bind(builder.Configuration.GetSection(PasswordRateLimitOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<PasswordRateLimitOptions>, PasswordRateLimitOptionsValidator>();
builder.Services.AddPasswordRateLimiter();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

var app = builder.Build();

app.Services.GetRequiredService<IDatabaseMigrator>().Migrate();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

app.MapFilesEndpoints();
app.MapHealthEndpoints();

app.Run();
