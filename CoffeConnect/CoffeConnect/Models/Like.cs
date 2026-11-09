namespace CoffeConnect.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int PublicacionId { get; set; }
        public string UserId { get; set; }
        public Publicacion Publicacion { get; set; }
    }
}
