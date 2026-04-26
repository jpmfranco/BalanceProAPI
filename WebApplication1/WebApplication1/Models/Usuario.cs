using System.Numerics;

namespace WebApplication1.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Genero { get; set; }
        public string Correo { get; set; }
        public long Celular  { get; set; }
        public string Contrasena { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }

    }
   
}
