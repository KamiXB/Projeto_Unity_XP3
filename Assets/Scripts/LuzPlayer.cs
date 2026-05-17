using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LuzPlayer : MonoBehaviour
{
    [Header("Referência da luz")]
    public Light2D luz;
    [Header("Radius")]
    [Tooltip("Base radius of the light. Can be modified by pickups at runtime.")]
    public float baseRadius = 1f;

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
        // initialize radius from baseRadius if possible
        if (luz != null)
        {
            luz.pointLightOuterRadius = baseRadius;
        }

        // Apply any persistent light radius powerups saved between lives
        if (PlayerPowerups.Instance != null)
        {
            PlayerPowerups.Instance.ApplyToLuzPlayer(this);
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

    // Public API to apply a radius multiplier. duration <= 0 means permanent.
    public void ApplyRadiusMultiplier(float multiplier, float duration)
    {
        if (luz == null) return;
        if (multiplier <= 0f) return;

        luz.pointLightOuterRadius *= multiplier;

        if (duration > 0f)
        {
            StartCoroutine(RadiusDuration(multiplier, duration));
        }
    }

    private System.Collections.IEnumerator RadiusDuration(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (luz != null) luz.pointLightOuterRadius /= multiplier;
    }

    // Optional helper to set radius multiplicatively (used by persistent PlayerPowerups)
    public void ApplyRadiusMultiplierPermanent(float multiplier)
    {
        if (luz == null) return;
        if (multiplier <= 0f) return;
        luz.pointLightOuterRadius *= multiplier;
    }
}
