using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class LightPulse : MonoBehaviour
{
    public float initialRadius = 0.25f;
    public float maxRadius = 3f;
    public float duration = 0.5f;
    public bool pulse = true;
    public Color color = new Color(1f, 1f, 1f, 0.9f);

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
            sr.color = color;
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

        Destroy(gameObject);
    }

    // 🔦 DETECTA INIMIGOS ENQUANTO O PULSO PASSA
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Inimigo"))
        {
            other.SendMessage(
                "AoReceberLuz",
                (Vector2)transform.position,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Inimigo"))
        {
            other.SendMessage("PararLuz", SendMessageOptions.DontRequireReceiver);
        }
    }
}