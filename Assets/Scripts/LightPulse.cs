using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LightPulse : MonoBehaviour
{
    [Tooltip("Initial radius (local scale multiplier)")]
    public float initialRadius = 0.25f;
    [Tooltip("Maximum radius (local scale multiplier)")]
    public float maxRadius = 3f;
    [Tooltip("Duration of pulse in seconds")]
    public float duration = 0.5f;
    [Tooltip("If true the light pulses (expands and fades). If false it stays constant at maxRadius")]
    public bool pulse = true;
    [Tooltip("Color of the light (alpha controls intensity)")]
    public Color color = new Color(1f,1f,1f,0.9f);

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("LightPulse requires a SpriteRenderer.");
            enabled = false;
            return;
        }

        // Ensure sprite is centered and uses default material
        sr.color = color;
        transform.localRotation = Quaternion.identity;

        if (pulse)
        {
            transform.localScale = Vector3.one * initialRadius;
            StartCoroutine(PulseRoutine());
        }
        else
        {
            transform.localScale = Vector3.one * maxRadius;
            sr.color = color; // full color
        }
    }

    private IEnumerator PulseRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            float k = t / duration;
            float scale = Mathf.Lerp(initialRadius, maxRadius, k);
            transform.localScale = Vector3.one * scale;

            Color c = color;
            c.a = Mathf.Lerp(color.a, 0f, k);
            sr.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        // After pulse ends, destroy the pulse object
        Destroy(gameObject);
    }
}
