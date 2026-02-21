using SegudoExamen.Models;
using Google.Cloud.Firestore;

namespace SegudoExamen.Services;

public class DataService
{
    private static List<Usuario> _usuarios = new();
    private static List<Libro> _libros = new();
    private static List<Prestamo> _prestamos = new();
    private static List<Reserva> _reservas = new();

    static DataService()
    {
        InitializeTestData();
    }

    public async Task<Usuario?> GetUsuarioByEmail(string email)
        => await Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == email));

    public async Task<Usuario?> GetUsuarioById(string id)
        => await Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

    public async Task AddUsuario(Usuario usuario)
    {
        _usuarios.Add(usuario);
        await Task.CompletedTask;
    }

    public async Task<List<Usuario>> GetUsuarios()
        => await Task.FromResult(_usuarios);

    public async Task UpdateUsuario(Usuario usuario)
    {
        var index = _usuarios.FindIndex(u => u.Id == usuario.Id);
        if (index != -1) _usuarios[index] = usuario;
        await Task.CompletedTask;
    }

    public async Task<List<Libro>> GetLibros()
        => await Task.FromResult(_libros);

    public async Task<Libro?> GetLibroById(string id)
        => await Task.FromResult(_libros.FirstOrDefault(l => l.Id == id));

    public async Task AddLibro(Libro libro)
    {
        _libros.Add(libro);
        await Task.CompletedTask;
    }

    public async Task UpdateLibro(Libro libro)
    {
        var index = _libros.FindIndex(l => l.Id == libro.Id);
        if (index != -1) _libros[index] = libro;
        await Task.CompletedTask;
    }

    public async Task DeleteLibro(string id)
    {
        _libros.RemoveAll(l => l.Id == id);
        await Task.CompletedTask;
    }

   
    public async Task<List<Prestamo>> GetPrestamos()
        => await Task.FromResult(_prestamos);

    public async Task<Prestamo?> GetPrestamoById(string id)
        => await Task.FromResult(_prestamos.FirstOrDefault(p => p.Id == id));

    public async Task AddPrestamo(Prestamo prestamo)
    {
        _prestamos.Add(prestamo);
        await Task.CompletedTask;
    }

    public async Task UpdatePrestamo(Prestamo prestamo)
    {
        var index = _prestamos.FindIndex(p => p.Id == prestamo.Id);
        if (index != -1) _prestamos[index] = prestamo;
        await Task.CompletedTask;
    }

    public async Task<int> CountPrestamosActivosUsuario(string usuarioId)
        => await Task.FromResult(_prestamos.Count(p => p.UsuarioId == usuarioId && p.Estado == "activo"));

 
    private static void InitializeTestData()
    {
        // Admin
        _usuarios.Add(new Usuario
        {
            Id = "admin-001",
            Nombre = "Administrador",
            Apellido = "Sistema",
            Correo = "admin@biblioteca.com",
            Contrasena = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Rol = "admin",
            Activo = true,
            Multas = 0
        });

        // Bibliotecario
        _usuarios.Add(new Usuario
        {
            Id = "biblio-001",
            Nombre = "Carlos",
            Apellido = "Gutiérrez",
            Correo = "bibliotecario@biblioteca.com",
            Contrasena = BCrypt.Net.BCrypt.HashPassword("biblio123"),
            Rol = "bibliotecario",
            Activo = true,
            Multas = 0
        });

        // Libros
        _libros.Add(new Libro
        {
            Id = "libro-001",
            Titulo = "Cien años de soledad",
            Autor = "Gabriel García Márquez",
            ISBN = "9788437604947",
            Categoria = "Novela",
            Editorial = "Sudamericana",
            AnioPublicacion = 1967,
            CopiasDisponibles = 3,
            CopiasTotal = 5,
            Estado = "disponible"
        });

        _libros.Add(new Libro
        {
            Id = "libro-002",
            Titulo = "El principito",
            Autor = "Antoine de Saint-Exupéry",
            ISBN = "9780156013924",
            Categoria = "Literatura infantil",
            Editorial = "Reynal & Hitchcock",
            AnioPublicacion = 1943,
            CopiasDisponibles = 0,
            CopiasTotal = 2,
            Estado = "agotado"
        });

        Console.WriteLine("Datos de prueba inicializados");
    }
}

public class Reserva
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; }
    public string LibroId { get; set; }
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "pendiente";
    public int Prioridad { get; set; } = 1;
}
