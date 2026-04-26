namespace WebApplication1.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public DateTime Fecha { get; set; }
        public int Monto { get; set; }
        public int IdUsuario { get; set; }

    }
}
