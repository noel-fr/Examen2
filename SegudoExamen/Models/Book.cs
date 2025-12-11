

using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace SegundoExamen.Models
{
    [FirestoreData]
    public class Book
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string Title { get; set; } = string.Empty; // titulo [cite: 35]
        [FirestoreProperty] public string Author { get; set; } = string.Empty; // autor [cite: 35]
        [FirestoreProperty] public string ISBN { get; set; } = string.Empty; // Único [cite: 35]
        [FirestoreProperty] public string Category { get; set; } = string.Empty; // categoria [cite: 35]
        [FirestoreProperty] public string Publisher { get; set; } = string.Empty; // editorial [cite: 35]
        [FirestoreProperty] public int PublicationYear { get; set; } // añoPublicacion [cite: 35]
        
        [FirestoreProperty] public int AvailableCopies { get; set; } // copiasDisponibles [cite: 35]
        [FirestoreProperty] public int TotalCopies { get; set; } // copiasTotal [cite: 35]
        
        [FirestoreProperty] public string Location { get; set; } = string.Empty; // ubicacion [cite: 35]
        [FirestoreProperty] public string Status { get; set; } = "disponible"; // disponible, agotado, en mantenimiento [cite: 35]
        [FirestoreProperty] public string Description { get; set; } = string.Empty; // descripcion [cite: 36]
        [FirestoreProperty] public DateTime EntryDate { get; set; } = DateTime.UtcNow; // fechaIngreso [cite: 36]
    }
}