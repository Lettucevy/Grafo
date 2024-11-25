using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class GerenciadorGrafo : MonoBehaviour
{
    public List<Vertice> vertices = new List<Vertice>();
    public LineRenderer prefabLinha;
    public TextMeshProUGUI textoFilaOuPilha;
    public Material materialLinha;

    private List<Aresta> arestas = new List<Aresta>();
    private Queue<Vertice> fila = new Queue<Vertice>();
    private Stack<Vertice> pilha = new Stack<Vertice>();
    private List<Vertice> visitados = new List<Vertice>();
    private bool usarBFS = true;
    private bool buscando = false;

    private void Start()
    {
        InicializarVertices();
        ConectarArestasAPartirDeConexoes();
        IniciarBusca();
    }

    private TextMeshPro CriarRotulo(string texto, Transform pai, Vector3 deslocamento)
    {
        GameObject objetoRotulo = new GameObject($"{texto}_Rotulo");
        objetoRotulo.transform.SetParent(pai, false);
        objetoRotulo.transform.localPosition = deslocamento;

        TextMeshPro rotulo = objetoRotulo.AddComponent<TextMeshPro>();
        rotulo.text = texto;
        rotulo.fontSize = 20;
        rotulo.color = Color.white;

        return rotulo;
    }

    private void InicializarVertices()
    {
        foreach (var vertice in vertices)
        {
            vertice.Inicializar($" {vertices.IndexOf(vertice) + 1}");
            TextMeshPro rotulo = CriarRotulo(vertice.nome, vertice.transform, new Vector3(9f, 1.5f, 0f));
            vertice.DefinirRotulo(rotulo);
        }
        Debug.Log("Vértices inicializados.");
    }

    private void ConectarArestasAPartirDeConexoes()
    {
        foreach (var vertice in vertices)
        {
            if (vertice.verticesConectados == null || vertice.verticesConectados.Count == 0)
                continue;

            foreach (var verticeConectado in vertice.verticesConectados)
            {
                if (!ArestaExiste(vertice, verticeConectado))
                {
                    CriarAresta($"Aresta {vertice.nome}-{verticeConectado.nome}", vertice, verticeConectado);
                }
            }
        }
    }

    private void CriarAresta(string nome, Vertice v1, Vertice v2)
    {
        LineRenderer linha = Instantiate(prefabLinha, transform);
        linha.material = materialLinha;
        linha.SetPosition(0, v1.transform.position);
        linha.SetPosition(1, v2.transform.position);
        Aresta aresta = new Aresta(nome, v1, v2, linha);
        arestas.Add(aresta);
    }

    private bool ArestaExiste(Vertice v1, Vertice v2)
    {
        return arestas.Exists(a =>
            (a.Vertice1 == v1 && a.Vertice2 == v2) || (a.Vertice1 == v2 && a.Vertice2 == v1));
    }

    private void IniciarBusca()
    {
        fila.Clear();
        pilha.Clear();
        visitados.Clear();

        if (vertices.Count > 0)
        {
            Vertice verticeInicial = vertices[0];
            if (usarBFS)
                fila.Enqueue(verticeInicial);
            else
                pilha.Push(verticeInicial);

            buscando = true;
            Debug.Log("Busca iniciada.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && buscando)
        {
            if (usarBFS)
            {
                PassoBFS();
            }
            else
            {
                PassoDFS();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReiniciarGrafo();
            IniciarBusca();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            usarBFS = !usarBFS;
            Debug.Log($"Alternando para {(usarBFS ? "BFS" : "DFS")}");
        }
    }

    private void PassoBFS()
    {
        if (fila.Count > 0)
        {
            Vertice atual = fila.Dequeue();

            if (!visitados.Contains(atual))
            {
                visitados.Add(atual);
                atual.DefinirCor(Color.red);
                Debug.Log($"Visitando: {atual.nome}");

                foreach (var vizinho in ObterVizinhos(atual))
                {
                    if (!visitados.Contains(vizinho) && !fila.Contains(vizinho))
                    {
                        fila.Enqueue(vizinho);
                    }
                }

                AtualizarTextoUI();
            }
            else
            {
                atual.DefinirCor(Color.gray);
            }
        }
        else
        {
            VerificarVerticesNaoVisitados();
        }
    }

    private void PassoDFS()
    {
        if (pilha.Count > 0)
        {
            Vertice atual = pilha.Pop();

            if (!visitados.Contains(atual))
            {
                visitados.Add(atual);
                atual.DefinirCor(Color.red);
                Debug.Log($"Visitando: {atual.nome}");

                foreach (var vizinho in ObterVizinhos(atual))
                {
                    if (!visitados.Contains(vizinho) && !pilha.Contains(vizinho))
                    {
                        pilha.Push(vizinho);
                    }
                }

                AtualizarTextoUI();
            }
            else
            {
                atual.DefinirCor(Color.gray);
            }
        }
        else
        {
            VerificarVerticesNaoVisitados();
        }
    }

    private void VerificarVerticesNaoVisitados()
    {
        foreach (var vertice in vertices)
        {
            if (!visitados.Contains(vertice))
            {
                Debug.Log($"Encontrado vértice não visitado: {vertice.nome}. Continuando busca...");
                if (usarBFS)
                    fila.Enqueue(vertice);
                else
                    pilha.Push(vertice);

                return;
            }
        }

        FinalizarBusca();
    }

    private void FinalizarBusca()
    {
        Debug.Log("Busca finalizada.");
        buscando = false;
    }

    private List<Vertice> ObterVizinhos(Vertice vertice)
    {
        return vertice.verticesConectados ?? new List<Vertice>();
    }

    private void AtualizarTextoUI()
    {
        if (usarBFS)
        {
            textoFilaOuPilha.text = $"Fila: {string.Join(", ", ObterNomesFormatados(fila))}";
        }
        else
        {
            textoFilaOuPilha.text = $"Pilha: {string.Join(", ", ObterNomesFormatados(pilha))}";
        }
    }

    private IEnumerable<string> ObterNomesFormatados(IEnumerable<Vertice> listaVertices)
    {
        foreach (var vertice in listaVertices)
        {
            yield return $"Vértice {vertice.nome}";
        }
    }

    private void ReiniciarGrafo()
    {
        foreach (var vertice in vertices)
        {
            vertice.DefinirCor(Color.green);
        }
        visitados.Clear();
        fila.Clear();
        pilha.Clear();
        buscando = false;
        Debug.Log("Grafo reiniciado.");
    }
}

public class Aresta
{
    public string Nome;
    public Vertice Vertice1;
    public Vertice Vertice2;
    public LineRenderer Linha;

    public Aresta(string nome, Vertice v1, Vertice v2, LineRenderer linha)
    {
        Nome = nome;
        Vertice1 = v1;
        Vertice2 = v2;
        Linha = linha;
    }
}
