
using Google.Cloud.Firestore;
using System;

namespace SegundoExamen.Models // Namespace: SegundoExamen
{
    [FirestoreData]
    public class Loan
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string UserId { get; set; } = string.Empty; 
        [FirestoreProperty] public string BookId { get; set; } = string.Empty; 
        [FirestoreProperty] public DateTime LoanDate { get; set; } = DateTime.UtcNow; 
        [FirestoreProperty] public DateTime ExpectedReturnDate { get; set; } 
        [FirestoreProperty] public DateTime? RealReturnDate { get; set; } = null; 
        [FirestoreProperty] public int DaysLate { get; set; } = 0; 
        [FirestoreProperty] public double FineGenerated { get; set; } = 0; 
        [FirestoreProperty] public string Status { get; set; } = "activo"; 
        [FirestoreProperty] public int Renewals { get; set; } = 0; 
    }
}