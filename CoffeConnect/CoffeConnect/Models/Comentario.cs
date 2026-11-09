namespace CoffeConnect.Models
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int PublicacionId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Publicacion Publicacion { get; set; }
    }
}
