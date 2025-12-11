using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace SegundoExamen.Services
{
    public class FirebaseServices
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _projectId;

        public FirebaseServices(IConfiguration configuration)
        {
            // Lee el ProjectId desde appsettings.json
            _projectId = configuration["Firebase:ProjectId"]
                         ?? throw new InvalidOperationException("Firebase ProjectID no configurado");

            // Inicializar Firebase App si no está inicializado
            if (FirebaseApp.DefaultInstance == null)
            {
                // Usa GOOGLE_APPLICATION_CREDENTIALS (como en el proyecto de clase)
                var credential = GoogleCredential.GetApplicationDefault();
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _projectId
                });
            }

            // Crear instancia de Firestore
            _firestoreDb = FirestoreDb.Create(_projectId);
        }

        public FirestoreDb GetFirestoreDb()
        {
            return _firestoreDb;
        }

        public CollectionReference GetCollection(string collectionName)
        {
            return _firestoreDb.Collection(collectionName);
        }
    }
}