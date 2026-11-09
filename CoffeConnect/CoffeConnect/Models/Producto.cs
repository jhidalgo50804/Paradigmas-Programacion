namespace CoffeConnect.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } // Stock
        public string? ImagenUrl { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string VendedorId { get; set; }
        public ApplicationUser Vendedor { get; set; }

        public bool Agotado => Cantidad <= 0;
    }
}
