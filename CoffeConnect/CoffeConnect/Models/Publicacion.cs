namespace CoffeConnect.Models
{
    public class Publicacion
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Tema { get; set; } 
        public string Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
