using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LightPulse : MonoBehaviour
{
    public float initialRadius = 0.25f;
    public float maxRadius = 3f;
    public float duration = 0.5f;
    public bool pulse = true;
    public Color color = new Color(1f, 1f, 1f, 0.9f);

    [Tooltip("How often (seconds) to check for enemies inside the light radius")]
    public float checkInterval = 0.12f;

    private SpriteRenderer sr;
    private HashSet<InimigoBase> tracked = new HashSet<InimigoBase>();

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

        // start checking for enemies in radius
        StartCoroutine(MonitorEnemies());
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

    private IEnumerator MonitorEnemies()
    {
        // run until destroyed
        while (true)
        {
            // find colliders in radius
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, maxRadius);

            // mark found enemies
            HashSet<InimigoBase> found = new HashSet<InimigoBase>();
            foreach (var c in cols)
            {
                if (c == null) continue;
                if (!c.CompareTag("Inimigo")) continue;

                var inim = c.GetComponentInParent<InimigoBase>();
                if (inim == null) continue;

                found.Add(inim);
                // notify
                inim.AoReceberLuz(transform.position, maxRadius);
            }

            // detect enemies that left the radius
            var toRemove = new List<InimigoBase>();
            foreach (var prev in tracked)
            {
                if (!found.Contains(prev))
                {
                    prev.PararLuz();
                    toRemove.Add(prev);
                }
            }

            // update tracked set
            foreach (var r in toRemove) tracked.Remove(r);
            foreach (var f in found) tracked.Add(f);

            yield return new WaitForSeconds(checkInterval);
        }
    }

    void OnDestroy()
    {
        // ensure we notify tracked enemies that light stopped
        foreach (var inim in tracked)
        {
            if (inim != null) inim.PararLuz();
        }
    }
}
