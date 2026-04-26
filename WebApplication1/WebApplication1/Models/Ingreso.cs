namespace WebApplication1.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public int Monto { get; set; }
        public int IdUsuario { get; set; }

    }
}
