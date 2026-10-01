using Server.Application;
using Server.Domain.Abstractions;
using Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.Services.GetRequiredService<IDatabaseMigrator>().Migrate();

app.MapGet("/", () => "Hello World!");

app.Run();
