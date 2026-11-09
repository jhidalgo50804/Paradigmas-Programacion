using CoffeConnect.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeConnect.Services
{
    public class GrafoService
    {
        private Dictionary<string, HashSet<string>> grafo = new();

        public void CargarDesdeDB(AppDbContext context)
        {
            grafo.Clear();
            var likes = context.Likes.Include(l => l.Publicacion).ToList();
            var comentarios = context.Comentarios.Include(c => c.Publicacion).ToList();

            foreach (var like in likes)
            {
                if (like.Publicacion == null) continue;
                var autorId = like.Publicacion.UserId;
                var likerId = like.UserId;
                if (autorId == likerId) continue;
                AgregarConexion(autorId, likerId);
            }
            foreach (var com in comentarios)
            {
                if (com.Publicacion == null) continue;
                var autorId = com.Publicacion.UserId;
                var comentaristaId = com.UserId;
                if (autorId == comentaristaId) continue;
                AgregarConexion(autorId, comentaristaId);
            }
        }

        private void AgregarConexion(string u1, string u2)
        {
            if (string.IsNullOrEmpty(u1) || string.IsNullOrEmpty(u2)) return;
            if (!grafo.ContainsKey(u1)) grafo[u1] = new HashSet<string>();
            if (!grafo.ContainsKey(u2)) grafo[u2] = new HashSet<string>();
            grafo[u1].Add(u2);
            grafo[u2].Add(u1);
        }

        public List<string> QuizasConozcas(string? miId)
        {
            if (string.IsNullOrEmpty(miId)) return new List<string>();
            if (!grafo.ContainsKey(miId)) return new List<string>();

            var visitados = new HashSet<string> { miId };
            var amigosDirectos = grafo[miId];
            foreach (var a in amigosDirectos) visitados.Add(a);

            var sugeridos = new HashSet<string>();
            var cola = new Queue<string>(amigosDirectos);

            while (cola.Count > 0)
            {
                var actual = cola.Dequeue();
                if (!grafo.ContainsKey(actual)) continue;
                foreach (var amigoDeAmigo in grafo[actual])
                {
                    if (!visitados.Contains(amigoDeAmigo))
                    {
                        sugeridos.Add(amigoDeAmigo);
                        visitados.Add(amigoDeAmigo);
                    }
                }
            }
            return sugeridos.ToList();
        }


        //para debug - no me sirve el grafo - no se por que
        public string GetDebugInfo(string? miId)
        {
            if (string.IsNullOrEmpty(miId))
                return "MiId NULL (no logueado)";

            if (!grafo.ContainsKey(miId))
                return $"No grafo. Total nodos: {grafo.Count}";

            return $"Tienes {grafo[miId].Count} amigos. Nodos totales: {grafo.Count}";
        }
    }
}