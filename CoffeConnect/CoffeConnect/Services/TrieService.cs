public class NodoTrie { public Dictionary<char, NodoTrie> Hijos = new(); public List<int> Ids = new(); }
public class TrieService
{
    private NodoTrie raiz = new();
    public void Insertar(string texto, int id)
    {
        if (string.IsNullOrWhiteSpace(texto)) return;
        var nodo = raiz;
        foreach (var c in texto.ToLower())
        {
            if (!nodo.Hijos.ContainsKey(c)) nodo.Hijos[c] = new();
            nodo = nodo.Hijos[c];
        }
        nodo.Ids.Add(id);
    }
    public List<int> Buscar(string prefijo)
    {
        var nodo = raiz;
        foreach (var c in prefijo.ToLower())
        {
            if (!nodo.Hijos.ContainsKey(c)) return new();
            nodo = nodo.Hijos[c];
        }
        return Recolectar(nodo);
    }
    private List<int> Recolectar(NodoTrie n)
    {
        var r = new List<int>(n.Ids);
        foreach (var h in n.Hijos.Values) r.AddRange(Recolectar(h));
        return r.Distinct().ToList();
    }
    public void Limpiar() { raiz = new NodoTrie(); }
}