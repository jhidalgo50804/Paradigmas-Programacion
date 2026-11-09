using CoffeConnect.Data;
using CoffeConnect.Models;
using CoffeConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeConnect.Controllers
{
    [Authorize]
    public class ComunidadController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly GrafoService _grafo;
        private readonly TrieService _trie;
        private readonly SeguridadService _seguridad;

        public ComunidadController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, GrafoService grafo, TrieService trie, SeguridadService seg)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _grafo = grafo;
            _trie = trie;
            _seguridad = seg;
        }

        public async Task<IActionResult> Index(string? q)
        {
            _grafo.CargarDesdeDB(_context);

            _trie.Limpiar();
            var todas = await _context.Publicaciones.ToListAsync();
            foreach (var p in todas) { _trie.Insertar(p.Titulo, p.Id); _trie.Insertar(p.Tema, p.Id); }

            List<Publicacion> pubs;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var ids = _trie.Buscar(q);
                pubs = await _context.Publicaciones.Include(p => p.User).Include(p => p.Likes).Include(p => p.Comentarios).ThenInclude(c => c.User)
                       .Where(p => ids.Contains(p.Id)).OrderByDescending(p => p.FechaCreacion).ToListAsync();
            }
            else
            {
                pubs = await _context.Publicaciones.Include(p => p.User).Include(p => p.Likes).Include(p => p.Comentarios).ThenInclude(c => c.User)
                       .OrderByDescending(p => p.FechaCreacion).ToListAsync();
            }

            var miId = _userManager.GetUserId(User);
            var idsSugeridos = _grafo.QuizasConozcas(miId);
            var usuariosSugeridos = await _context.Users.Where(u => idsSugeridos.Contains(u.Id)).ToListAsync();

            ViewBag.Sugeridos = usuariosSugeridos;
            ViewBag.Query = q;
            ViewBag.DebugGrafo = _grafo.GetDebugInfo(miId);
            ViewBag.MiId = miId;

            return View(pubs);
        }

        [HttpGet]
        public IActionResult BuscarTrie(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Json(new List<int>());
            var ids = _trie.Buscar(q);
            var resultados = _context.Publicaciones
                .Where(p => ids.Contains(p.Id))
                .Select(p => new { id = p.Id, titulo = p.Titulo })
                .Take(5).ToList();
            return Json(resultados);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(string Titulo, string Tema, string Descripcion, IFormFile Imagen)
        {
            var user = await _userManager.GetUserAsync(User);
            string? url = null;
            if (Imagen != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(Imagen.FileName);
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "uploads"));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Imagen.CopyToAsync(stream);
                }
                url = "/uploads/" + fileName;
            }
            var pub = new Publicacion { Titulo = Titulo, Tema = Tema, Descripcion = Descripcion, ImagenUrl = url, UserId = user.Id };
            _context.Add(pub);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Tabla Hash
            if (_seguridad.EsLikeDuplicado(userId, id)) return RedirectToAction("Index");

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return RedirectToAction("Index");

            var existe = await _context.Likes.FirstOrDefaultAsync(l => l.PublicacionId == id && l.UserId == userId);

            // Grafo no dirigido        
            if (existe == null)
                _context.Likes.Add(new Like { PublicacionId = id, UserId = userId });
            else
                _context.Likes.Remove(existe);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Comentar(int publicacionId, string texto)
        {
            var userId = _userManager.GetUserId(User);

            // Tabla Hash
            if (!_seguridad.PuedeComentar(userId))
            {
                TempData["Error"] = "Espera 10 segundos entre comentarios.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(texto)) return RedirectToAction("Index");

            _context.Comentarios.Add(new Comentario
            {
                PublicacionId = publicacionId,
                Texto = texto,
                UserId = userId
            });

            var pub = await _context.Publicaciones.FindAsync(publicacionId);
            if (pub != null)
              

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}