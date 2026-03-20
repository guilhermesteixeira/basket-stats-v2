using BasketStats.Domain.Abstractions;
using BasketStats.Infrastructure.Repositories;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasketStats.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["Firestore:ProjectId"] ?? "basket-stats-dev";

        services.AddSingleton(_ =>
        {
            var emulatorHost = configuration["Firestore:EmulatorHost"];
            if (!string.IsNullOrEmpty(emulatorHost))
            {
                Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", emulatorHost);
            }
            return FirestoreDb.Create(projectId);
        });

        services.AddScoped<IMatchRepository, FirestoreMatchRepository>();
        services.AddScoped<ITeamRepository, FirestoreTeamRepository>();
        services.AddScoped<IUserRepository, FirestoreUserRepository>();

        return services;
    }
}
