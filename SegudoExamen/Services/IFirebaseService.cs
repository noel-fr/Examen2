using Google.Cloud.Firestore;

namespace SegundoExamen.Services.Interfaces
{
    public interface IFirebaseService
    {
        FirestoreDb GetFirestoreDb();
        CollectionReference GetCollection(string collectionName);
    }
}