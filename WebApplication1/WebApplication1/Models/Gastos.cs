namespace WebApplication1.Models
{
    public class Gastos
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public int IdUsuario { get; set; }
        public string? Clasificacion { get; set; }


    }
}
