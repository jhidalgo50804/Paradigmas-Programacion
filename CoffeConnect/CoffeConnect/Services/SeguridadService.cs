namespace CoffeConnect.Services
{
    public class SeguridadService
    {
        // Tabla Hash 1 para comentarios
        private Dictionary<string, DateTime> ultimosComentarios = new();
        // Tabla Hash 2 evita like instantáneo
        private HashSet<string> cacheLikes = new();

        public bool PuedeComentar(string userId)
        {
            if (ultimosComentarios.TryGetValue(userId, out var fecha))
                if ((DateTime.Now - fecha).TotalSeconds < 10) return false;
            ultimosComentarios[userId] = DateTime.Now;
            return true;
        }
        public bool EsLikeDuplicado(string userId, int pubId)
        {
            var clave = $"{userId}-{pubId}";
            if (cacheLikes.Contains(clave)) return true;
            cacheLikes.Add(clave);
            Task.Delay(2000).ContinueWith(t => cacheLikes.Remove(clave)); 
            return false;
        }
    }
}
