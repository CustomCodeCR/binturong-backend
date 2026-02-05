using Api.Extensions;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================
builder.Services.AddSwaggerWithJwt();
builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// =========================
// Dev / Infra bootstrap
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithJwt();
    await app.ApplyDevMigrationsAndMongoAsync();
}

// =========================
// Scope seeding (ALL envs)
// =========================
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

// =========================
// Middleware
// =========================
app.UseHttpsRedirection();

// Auth MUST be before endpoints
app.UseAuthentication();
app.UseAuthorization();

// =========================
// Endpoints
// =========================
app.MapEndpoints();
app.MapGet("/health", () => Results.Ok("OK")).WithTags("Health");

app.Run();
