using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CoffeConnect.Models; 

namespace CoffeConnect.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Publicacion> Publicaciones { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Compra> Compras { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Múltiples cascadas
            builder.Entity<Publicacion>()
                .HasOne(p => p.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comentario>()
                .HasOne(c => c.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comentario>()
                .HasOne(c => c.Publicacion)
                .WithMany(p => p.Comentarios)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.Publicacion)
                .WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Producto>().HasOne(p => p.Vendedor).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Compra>().HasOne(c => c.Comprador).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Compra>().HasOne(c => c.Producto).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}