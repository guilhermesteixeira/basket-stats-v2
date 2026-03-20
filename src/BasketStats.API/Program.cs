using System.Security.Claims;
using BasketStats.Application;
using BasketStats.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // JWT Bearer Authentication (Keycloak)
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Keycloak:Authority"];
            options.Audience = builder.Configuration["Keycloak:Audience"];
            options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RoleClaimType = "roles",
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

