using System.Numerics;

namespace WebApplication1.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public long Celular  { get; set; }
        public string Contrasena { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }

    }
   
}
