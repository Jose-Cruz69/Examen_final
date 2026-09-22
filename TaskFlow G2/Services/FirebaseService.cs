using Google.Cloud.Firestore;

namespace TaskFlow_G2.Services;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;

    public FirebaseService(IConfiguration configuration)
    {
        var credentialPath = Path.Combine(
            AppContext.BaseDirectory, "Config", "firebase-credentials.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

        var projectId = configuration["Firebase:ProjectId"]
                        ?? throw new InvalidOperationException("Falta 'Firebase:ProjectId' en appsettings.json.");

        _firestoreDb = FirestoreDb.Create(projectId);
    }

    public CollectionReference GetCollection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
}