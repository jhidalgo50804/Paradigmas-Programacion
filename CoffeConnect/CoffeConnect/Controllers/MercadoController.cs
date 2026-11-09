using CoffeConnect.Data;
using CoffeConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MercadoController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    public MercadoController(AppDbContext c, UserManager<ApplicationUser> u, IWebHostEnvironment e)
    { _context = c; _userManager = u; _env = e; }

    public async Task<IActionResult> Index()
    {
        var prods = await _context.Productos.Include(p => p.Vendedor).OrderByDescending(p => p.FechaCreacion).ToListAsync();
        return View(prods);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(string Nombre, string Descripcion, decimal Precio, int Cantidad, IFormFile Imagen)
    {
        var user = await _userManager.GetUserAsync(User);
        string? url = null;
        if (Imagen != null)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(Imagen.FileName);
            var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
            Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "uploads"));
            using (var fs = new FileStream(path, FileMode.Create)) await Imagen.CopyToAsync(fs);
            url = "/uploads/" + fileName;
        }
        _context.Productos.Add(new Producto { Nombre = Nombre, Descripcion = Descripcion, Precio = Precio, Cantidad = Cantidad, ImagenUrl = url, VendedorId = user.Id });
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Pagar(int id)
    {
        var prod = await _context.Productos.Include(p => p.Vendedor).FirstOrDefaultAsync(p => p.Id == id);
        if (prod == null || prod.Agotado) return RedirectToAction("Index");
        return View(prod);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmarCompra(int id)
    {
        var prod = await _context.Productos.FindAsync(id);
        if (prod == null || prod.Cantidad <= 0) return RedirectToAction("Index");

        prod.Cantidad -= 1; // Compra solo 1

        _context.Compras.Add(new Compra
        {
            ProductoId = prod.Id,
            CompradorId = _userManager.GetUserId(User),
            CantidadComprada = 1,
            TotalPagado = prod.Precio
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"¡Compraste {prod.Nombre} por ₡{prod.Precio}!";
        return RedirectToAction("MisProductos");
    }

    public async Task<IActionResult> MisProductos()
    {
        var userId = _userManager.GetUserId(User);
        var compras = await _context.Compras.Include(c => c.Producto).Where(c => c.CompradorId == userId).OrderByDescending(c => c.FechaCompra).ToListAsync();
        return View(compras);
    }
}