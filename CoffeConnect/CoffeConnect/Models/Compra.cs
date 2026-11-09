namespace CoffeConnect.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public string CompradorId { get; set; }
        public ApplicationUser Comprador { get; set; }
        public int CantidadComprada { get; set; } = 1;
        public decimal TotalPagado { get; set; }
        public DateTime FechaCompra { get; set; } = DateTime.Now;
    }
}
