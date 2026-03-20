namespace BasketStats.Infrastructure.Tests.Integration;

using Google.Api.Gax;
using Google.Cloud.Firestore;

public class FirestoreIntegrationFixture : IAsyncLifetime
{
    public FirestoreDb? FirestoreDb { get; private set; }
    public bool IsAvailable { get; private set; }
    public const string ProjectId = "basket-stats-test";

    public async Task InitializeAsync()
    {
        var emulatorHost = Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")
                           ?? "localhost:8081";
        try
        {
            Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", emulatorHost);
            FirestoreDb = new FirestoreDbBuilder
            {
                ProjectId = ProjectId,
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.Build();
            // Quick connectivity check
            await FirestoreDb.Collection("_health").Document("check").GetSnapshotAsync();
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
