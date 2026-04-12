using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LuzSafeRoom : MonoBehaviour
{
    [Header("Referência da luz")]
    public Light2D luz;

    [Header("Configuração da respiração")]
    public float intensidadeBase = 1.5f;
    public float variacao = 0.2f;
    public float velocidade = 2f;

    void Start()
    {
        // Caso você esqueça de setar no inspector
        if (luz == null)
        {
            luz = GetComponent<Light2D>();
        }
    }

    void Update()
    {
        if (luz == null) return;

        float pulso = Mathf.Sin(Time.time * velocidade) * variacao;
        luz.intensity = intensidadeBase + pulso;
    }
}