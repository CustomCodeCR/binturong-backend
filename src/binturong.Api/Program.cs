using Api.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Notifications;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendCors",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "http://127.0.0.1:5173",
                    "https://127.0.0.1:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddSwaggerWithJwt();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithJwt();
    await app.ApplyDevMigrationsAndMongoAsync();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await services
        .GetRequiredService<Infrastructure.Database.Postgres.Seed.ScopeSeeder>()
        .SeedAsync();
    await services
        .GetRequiredService<Infrastructure.Database.Postgres.Seed.RoleSeeder>()
        .SeedAsync();
    await services
        .GetRequiredService<Infrastructure.Database.Postgres.Seed.AdminUserSeeder>()
        .SeedAsync();
}

app.UseHttpsRedirection();

app.UseCors("FrontendCors");

var localRootPath = builder.Configuration["Storage:Local:RootPath"];

if (!string.IsNullOrWhiteSpace(localRootPath))
{
    var absoluteRootPath = Path.GetFullPath(localRootPath);

    Directory.CreateDirectory(absoluteRootPath);

    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(absoluteRootPath),
            RequestPath = "/storage",
        }
    );
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapGet("/health", () => Results.Ok("OK")).WithTags("Health");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
