using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpeedPickup : MonoBehaviour
{
    [Tooltip("Multiplicador aplicado à velocidade do jogador. >1 aumenta a velocidade.")]
    [SerializeField] private float speedMultiplier = 1.5f;

    [Tooltip("Duração do efeito em segundos. <=0 significa permanente.")]
    [SerializeField] private float duration = 0f;

    [Header("Optional Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool destroyOnCollect = true;

    private bool collected = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (other == null) return;

        // Try to find any component in the parent hierarchy that exposes ApplySpeedMultiplier
        var comps = other.GetComponentsInParent<MonoBehaviour>(true);
        MethodInfo method = null;
        Component targetComp = null;
        foreach (var c in comps)
        {
            if (c == null) continue;
            method = c.GetType().GetMethod("ApplySpeedMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                targetComp = c;
                break;
            }
        }

        if (method == null || targetComp == null)
        {
            Debug.LogWarning($"SpeedPickup: nenhum componente com ApplySpeedMultiplier encontrado em '{other.gameObject.name}' ou seus pais.");
            return;
        }

        collected = true;

        // If duration <= 0 treat as permanent: save in PlayerPowerups so it persists across deaths
        if (duration <= 0f)
        {
            // Save persistent upgrade via reflection to avoid direct type dependency
            System.Type ppType = null;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                ppType = asm.GetType("PlayerPowerups");
                if (ppType != null) break;
            }

            if (ppType != null)
            {
                var instanceProp = ppType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    var setMethod = ppType.GetMethod("SetSpeedUpgrade", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (setMethod != null && instance != null)
                    {
                        setMethod.Invoke(instance, new object[] { speedMultiplier });
                        Debug.Log($"SpeedPickup: saved persistent speed upgrade x{speedMultiplier} via PlayerPowerups");
                    }
                }
            }

            method.Invoke(targetComp, new object[] { speedMultiplier, 0f });
            Debug.Log($"SpeedPickup: aplicado permanentemente multiplicador x{speedMultiplier} em '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }
        else
        {
            // Temporary effect
            method.Invoke(targetComp, new object[] { speedMultiplier, duration });
            Debug.Log($"SpeedPickup: aplicado multiplicador x{speedMultiplier} por {duration} segundos em '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (pickupSound != null)
        {
            var camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, camPos);
        }

        if (destroyOnCollect) Destroy(gameObject);
        collected = false;
    }
}
