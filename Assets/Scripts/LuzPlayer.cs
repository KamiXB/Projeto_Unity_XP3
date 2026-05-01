using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LuzPlayer : MonoBehaviour
{
    [Header("Referência da luz")]
    public Light2D luz;

    [Header("Seguir")]
    [Tooltip("Transform do jogador a seguir. Se vazio, procura GameObject com tag 'Player'.")]
    public Transform target;
    [Tooltip("Offset local em relação ao jogador")]
    public Vector3 offset = Vector3.zero;
    [Tooltip("Se true, atualiza posição no LateUpdate (recomendado)")]
    public bool followInLateUpdate = true;

    [Header("Pulso")]
    public float intensidadeBase = 1.5f;
    public float variacao = 0.2f;
    public float velocidade = 2f;

    void Start()
    {
        if (luz == null)
        {
            luz = GetComponent<Light2D>();
        }

        if (target == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) target = go.transform;
        }
    }

    void Update()
    {
        if (luz == null) return;

        float pulso = Mathf.Sin(Time.time * velocidade) * variacao;
        luz.intensity = intensidadeBase + pulso;

        if (!followInLateUpdate)
        {
            UpdatePosition();
        }
    }

    void LateUpdate()
    {
        if (followInLateUpdate)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        if (target == null) return;

        // Keep the light's transform at the player's position + offset
        luz.transform.position = target.position + offset;
    }
}
