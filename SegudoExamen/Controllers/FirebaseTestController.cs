
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.Services;

namespace SegudoExamen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirebaseTestController : ControllerBase
    {
        private readonly FirebaseServices _firebaseServices8;

        public FirebaseTestController(FirebaseServices firebaseServices8)
        {
            _firebaseServices8 = firebaseServices8;
        }

        /// <summary>
        /// Prueba la conexión con Firebase / Firestore.
        /// </summary>
        [HttpGet("conexion")]
        public async Task<IActionResult> ProbarConexion()
        {
            try
            {
                // Simplemente intentamos obtener el objeto FirestoreDb
                var db = _firebaseServices8.GetFirestoreDb();

                // Si llega aquí sin excepción, la conexión es correcta
                return Ok(new
                {
                    mensaje = "Conexión exitosa a Firebase",
                    proyecto = db.ProjectId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al conectar con Firebase",
                    detalle = ex.Message
                });
            }
        }
    }
}