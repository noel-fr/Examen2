using Google.Cloud.Firestore;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        public CollectionReference GetCollection(string collectionName)
        {
            return _firestoreDb.Collection(collectionName);
        }
    }
}