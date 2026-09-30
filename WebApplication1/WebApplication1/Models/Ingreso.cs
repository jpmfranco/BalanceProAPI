namespace WebApplication1.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public int IdUsuario { get; set; }

    }
}
