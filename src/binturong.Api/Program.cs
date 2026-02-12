using Api.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Notifications;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapGet("/health", () => Results.Ok("OK")).WithTags("Health");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
