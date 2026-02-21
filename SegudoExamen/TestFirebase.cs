using Google.Cloud.Firestore;
using System;

public class TestFirebase
{
    public static void Test()
    {
        try
        {
            string credentialPath = "firebase-credentials.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

            var db = FirestoreDb.Create("examen2-77402");
            Console.WriteLine("✅ Firebase conectado exitosamente!");
            Console.WriteLine($"Project: {db.ProjectId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}