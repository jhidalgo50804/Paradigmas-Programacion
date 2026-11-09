using Microsoft.AspNetCore.Identity;

namespace CoffeConnect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = "";
        public string Region { get; set; } = "";
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}