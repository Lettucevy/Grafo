using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GerenciadorGrafo : MonoBehaviour
{
    public List<Vertice> vertices = new List<Vertice>();
    public LineRenderer prefabLinha;
    public Material materialLinha;
    public TextMeshProUGUI textoFilaPilha;

    public Vertice pontoInicial;
    public Vertice pontoFinal;

    private List<Aresta> arestas = new List<Aresta>();
    private Queue<Vertice> fila = new Queue<Vertice>();
    private Stack<Vertice> pilha = new Stack<Vertice>();
    private List<Vertice> visitados = new List<Vertice>();
    private bool isBuscando = false;

    private enum AlgoritmoBusca { BFS, BFS_Prioridade, DFS, A_Star }
    private AlgoritmoBusca algoritmoAtual = AlgoritmoBusca.BFS;

    private void Start()
    {
        InicializarVertices();
        ConectarArestas();
        IniciarBusca();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isBuscando)
        {
            switch (algoritmoAtual)
            {
                case AlgoritmoBusca.BFS: PassoBFS(); break;
                case AlgoritmoBusca.BFS_Prioridade: PassoBFSPrioridade(); break;
                case AlgoritmoBusca.DFS: PassoDFS(); break;
                case AlgoritmoBusca.A_Star: PassoAStar(); break;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetarGrafo();
            IniciarBusca();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            AlternarAlgoritmo();
        }
    }

    private void AlternarAlgoritmo()
    {
        algoritmoAtual = (AlgoritmoBusca)(((int)algoritmoAtual + 1) % 4);
        Debug.Log($"Algoritmo atual: {algoritmoAtual}");
    }

    private void InicializarVertices()
    {
        foreach (var vertice in vertices)
        {
            vertice.Inicializar($"Vértice {vertices.IndexOf(vertice) + 1}");
            TextMeshPro rotulo = CriarRotulo(vertice.nome, vertice.transform, new Vector3(8f, 1.5f, 0));
            vertice.DefinirRotulo(rotulo);
        }
        Debug.Log("Vértices inicializados.");
    }

    private TextMeshPro CriarRotulo(string texto, Transform pai, Vector3 deslocamento)
    {
        GameObject objRotulo = new GameObject($"Rotulo_{texto}");
        objRotulo.transform.SetParent(pai, false);
        objRotulo.transform.localPosition = deslocamento;

        TextMeshPro rotulo = objRotulo.AddComponent<TextMeshPro>();
        rotulo.text = texto;
        rotulo.fontSize = 20;
        rotulo.color = Color.white;

        return rotulo;
    }

    private void ConectarArestas()
    {
        foreach (var vertice in vertices)
        {
            if (vertice.verticesConectados == null || vertice.verticesConectados.Count == 0)
                continue;

            foreach (var conectado in vertice.verticesConectados)
            {
                if (!ArestaExiste(vertice, conectado))
                {
                    CriarAresta($"Aresta {vertice.nome}-{conectado.nome}", vertice, conectado);
                }
            }
        }
    }

    private bool ArestaExiste(Vertice v1, Vertice v2)
    {
        return arestas.Exists(a => (a.vertice1 == v1 && a.vertice2 == v2) || (a.vertice1 == v2 && a.vertice2 == v1));
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

    private void IniciarBusca()
    {
        fila.Clear();
        pilha.Clear();
        visitados.Clear();

        if (vertices.Count > 0)
        {
            if (algoritmoAtual == AlgoritmoBusca.A_Star)
            {
                if (pontoInicial != null && pontoFinal != null)
                {
                    fila.Enqueue(pontoInicial);
                    isBuscando = true;
                }
            }
            else
            {
                Vertice verticeInicial = vertices.FirstOrDefault(v => !visitados.Contains(v));
                if (verticeInicial != null)
                {
                    if (algoritmoAtual == AlgoritmoBusca.BFS || algoritmoAtual == AlgoritmoBusca.BFS_Prioridade)
                        fila.Enqueue(verticeInicial);
                    else
                        pilha.Push(verticeInicial);

                    isBuscando = true;
                }
            }
        }
    }

    private void PassoBFS()
    {
        if (fila.Count > 0)
        {
            Vertice atual = fila.Dequeue();
            VisitarVertice(atual);
            foreach (var vizinho in ObterVizinhos(atual))
            {
                if (!visitados.Contains(vizinho) && !fila.Contains(vizinho))
                {
                    fila.Enqueue(vizinho);
                }
            }
            AtualizarTexto();
        }
        else
        {
            VerificarProximoInicio();
        }
    }

    private void PassoBFSPrioridade()
    {
        if (fila.Count > 0)
        {
            Vertice atual = fila.OrderByDescending(v => v.prioridade).First();
            fila = new Queue<Vertice>(fila.Where(v => v != atual));
            VisitarVertice(atual);

            foreach (var vizinho in ObterVizinhos(atual))
            {
                if (!visitados.Contains(vizinho) && !fila.Contains(vizinho))
                {
                    fila.Enqueue(vizinho);
                }
            }
            AtualizarTexto();
        }
        else
        {
            VerificarProximoInicio();
        }
    }

    private void PassoDFS()
    {
        if (pilha.Count > 0)
        {
            Vertice atual = pilha.Pop();
            VisitarVertice(atual);
            foreach (var vizinho in ObterVizinhos(atual))
            {
                if (!visitados.Contains(vizinho) && !pilha.Contains(vizinho))
                {
                    pilha.Push(vizinho);
                }
            }
            AtualizarTexto();
        }
        else
        {
            VerificarProximoInicio();
        }
    }

    private void PassoAStar()
    {
        if (fila.Count > 0)
        {
            Vertice atual = fila.OrderBy(v => Vector3.Distance(v.transform.position, pontoFinal.transform.position)).First();
            fila = new Queue<Vertice>(fila.Where(v => v != atual));
            VisitarVertice(atual);

            if (atual == pontoFinal)
            {
                Debug.Log("Caminho encontrado!");
                isBuscando = false;
                return;
            }

            foreach (var vizinho in ObterVizinhos(atual))
            {
                if (!visitados.Contains(vizinho) && !fila.Contains(vizinho))
                {
                    fila.Enqueue(vizinho);
                }
            }
            AtualizarTexto();
        }
        else
        {
            Debug.Log("Nenhum caminho encontrado.");
            isBuscando = false;
        }
    }

    private void VerificarProximoInicio()
    {
        Vertice proximo = vertices.FirstOrDefault(v => !visitados.Contains(v));
        if (proximo != null)
        {
            if (algoritmoAtual == AlgoritmoBusca.BFS || algoritmoAtual == AlgoritmoBusca.BFS_Prioridade)
                fila.Enqueue(proximo);
            else
                pilha.Push(proximo);

            Debug.Log($"Iniciando nova busca a partir de {proximo.nome}");
        }
        else
        {
            Debug.Log("Busca finalizada.");
            isBuscando = false;
        }
    }

    private void VisitarVertice(Vertice vertice)
    {
        if (!visitados.Contains(vertice))
        {
            visitados.Add(vertice);
            vertice.DefinirCor(Color.red);
            Debug.Log($"Visitando: {vertice.nome}");
        }
    }

    private List<Vertice> ObterVizinhos(Vertice vertice)
    {
        return vertice.verticesConectados ?? new List<Vertice>();
    }

    private void AtualizarTexto()
    {
        if (algoritmoAtual == AlgoritmoBusca.BFS || algoritmoAtual == AlgoritmoBusca.BFS_Prioridade || algoritmoAtual == AlgoritmoBusca.A_Star)
        {
            textoFilaPilha.text = $"Fila: {string.Join(", ", fila.Select(v => v.nome))}";
        }
        else
        {
            textoFilaPilha.text = $"Pilha: {string.Join(", ", pilha.Select(v => v.nome))}";
        }
    }

    private void ResetarGrafo()
    {
        fila.Clear();
        pilha.Clear();
        visitados.Clear();

        foreach (var vertice in vertices)
        {
            vertice.DefinirCor(Color.green);
        }

        Debug.Log("Grafo resetado.");
    }

    public class Aresta {

        public string nome;
        public Vertice vertice1; 
        public Vertice vertice2; 
        public LineRenderer linha; 

        public Aresta(string nome, Vertice v1, Vertice v2, LineRenderer linhaRenderer)
        {
            this.nome = nome;
            vertice1 = v1;
            vertice2 = v2;
            linha = linhaRenderer;
            AtualizarLinha();
        }
        public void AtualizarLinha()
        {
        if (linha != null && vertice1 != null && vertice2 != null)
            {
                linha.SetPosition(0, vertice1.transform.position);
                linha.SetPosition(1, vertice2.transform.position);
            }
        }

   
        public void DefinirCor(Color cor)
        {
            if (linha != null)
            {
                linha.material.color = cor;
            }
        }
    }

}
