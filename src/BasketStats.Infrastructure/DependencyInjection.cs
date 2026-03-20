using BasketStats.Domain.Abstractions;
using BasketStats.Infrastructure.Repositories;
using Google.Api.Gax;
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
            var useEmulator = !string.IsNullOrEmpty(emulatorHost);

            if (useEmulator)
                Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", emulatorHost);
            return new FirestoreDbBuilder
            {
                ProjectId = projectId,
                EmulatorDetection = useEmulator
                    ? EmulatorDetection.EmulatorOnly
                    : EmulatorDetection.None
            }.Build();
        });

        services.AddScoped<IMatchRepository, FirestoreMatchRepository>();
        services.AddScoped<ITeamRepository, FirestoreTeamRepository>();
        services.AddScoped<IUserRepository, FirestoreUserRepository>();

        return services;
    }
}
