

using Google.Cloud.Firestore;
using System;

namespace SegundoExamen.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        
        [FirestoreProperty] public string Name { get; set; } = string.Empty; // Nuevo 
        [FirestoreProperty] public string Lastname { get; set; } = string.Empty; // Nuevo 
        [FirestoreProperty] public string Email { get; set; } = string.Empty; // Único 
        // [Contraseña hasheada se almacena aparte en Firestore, como en VotoSeguro]
        
        [FirestoreProperty] public int Age { get; set; } // Nuevo 
        [FirestoreProperty] public string IdentityNumber { get; set; } = string.Empty; // Nuevo (numeroIdentidad) 
        [FirestoreProperty] public string Phone { get; set; } = string.Empty; // Nuevo (telefono) 
        
        [FirestoreProperty] public string Role { get; set; } = "usuario"; // Roles: usuario, bibliotecario, admin 
        [FirestoreProperty] public bool IsActive { get; set; } = true; // activo 
        [FirestoreProperty] public DateTime RegistrationDate { get; set; } = DateTime.UtcNow; // fechaRegistro 
        
        [FirestoreProperty] public double Fines { get; set; } = 0; // multas (Lempiras) 
    }
}