using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/sistema")]
public class SistemaController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            proyecto = "Sistema de Gestión Bibliotecaria",
            backend = "ASP.NET Core 8",
            autenticacion = "JWT con BCrypt",
            baseDatos = "Firestore (simulada para desarrollo)",
            estado = "Funcional",
            timestamp = DateTime.UtcNow
        });
    }
}