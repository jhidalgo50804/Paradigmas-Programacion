using CoffeConnect.Data;
using CoffeConnect.Models;
using CoffeConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace CoffeConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GrafoService _grafo;
        private readonly TrieService _trie;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager, GrafoService grafo, TrieService trie)
        {
            _context = context;
            _userManager = userManager;
            _grafo = grafo;
            _trie = trie;
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
            if (string.IsNullOrWhiteSpace(q)) return Json(new List<object>());
            var ids = _trie.Buscar(q);
            var resultados = _context.Publicaciones.Where(p => ids.Contains(p.Id)).Select(p => new { id = p.Id, titulo = p.Titulo }).Take(5).ToList();
            return Json(resultados);
        }
    }
}