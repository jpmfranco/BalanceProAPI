namespace WebApplication1.Models
{
    public class PerfilFinanciero
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int Perfil { get; set; }
        public int Estabilidad { get; set; }
        public int Categoria { get; set; }
        public int TendIngresos { get; set; }
        public int TendEgresos { get; set; }
        public int Control { get; set; }
        public int Planificacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
