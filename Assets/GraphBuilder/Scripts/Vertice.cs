using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Vertice : MonoBehaviour
{
    public string nome;
    public int prioridade;

    public List<Vertice> verticesConectados;

    private Renderer rendererVertice;
    private TextMeshPro rotulo;

    private void Awake()
    {
        rendererVertice = GetComponent<Renderer>();
    }

    public void Inicializar(string nomeGerado)
    {
        
        if (string.IsNullOrWhiteSpace(nome))
        {
            nome = nomeGerado;
        }

        AtualizarRotulo();
    }

    public void DefinirCor(Color cor)
    {
        if (rendererVertice != null)
        {
            rendererVertice.material.color = cor;
        }
    }

    public void DefinirRotulo(TextMeshPro novoRotulo)
    {
        rotulo = novoRotulo;
        AtualizarRotulo();
    }

    private void AtualizarRotulo()
    {
        if (rotulo != null)
        {
            rotulo.text = $"{nome} (P: {prioridade})";
        }
    }

    private void OnValidate()
    {
        
        if (!string.IsNullOrEmpty(nome))
        {
            AtualizarRotulo();
        }
    }
}
