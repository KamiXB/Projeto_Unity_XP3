using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FireRatePickup : MonoBehaviour
{
    [Tooltip("Multiplicador aplicado à cadência de tiro. >1 aumenta a cadência.")]
    [SerializeField] private float fireRateMultiplier = 1.5f;

    [Tooltip("Duração do efeito em segundos. <=0 significa permanente.")]
    [SerializeField] private float duration = 5f;

    [Header("Optional Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool destroyOnCollect = true;

    private bool collected = false;

    // Ensure collider is trigger by default when added in editor
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (other == null) return;

        // Try to find any component in the parent hierarchy that exposes ApplyFireRateMultiplier
        var comps = other.GetComponentsInParent<MonoBehaviour>(true);
        MethodInfo method = null;
        Component targetComp = null;
        foreach (var c in comps)
        {
            if (c == null) continue;
            method = c.GetType().GetMethod("ApplyFireRateMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                targetComp = c;
                break;
            }
        }

        if (method == null || targetComp == null)
        {
            Debug.LogWarning($"FireRatePickup: nenhum componente com ApplyFireRateMultiplier encontrado em '{other.gameObject.name}' ou seus pais.");
            return;
        }

        collected = true;

        // If duration <= 0 treat as permanent: save in PlayerPowerups so it persists across deaths
        if (duration <= 0f)
        {
            // Save persistent upgrade
            PlayerPowerups.Instance.SetFireRateUpgrade(fireRateMultiplier);

            // Apply immediately to the touched component
            method.Invoke(targetComp, new object[] { fireRateMultiplier, 0f });
            Debug.Log($"FireRatePickup: aplicado permanentemente multiplicador x{fireRateMultiplier} em '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }
        else
        {
            // Invoke the method using reflection for temporary effect
            method.Invoke(targetComp, new object[] { fireRateMultiplier, duration });
            Debug.Log($"FireRatePickup: aplicado multiplicador x{fireRateMultiplier} por {duration} segundos em '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Show UI message
        if (PickupUI.Instance != null)
        {
            PickupUI.Instance.ShowMessage($"Fire Rate x{fireRateMultiplier}");
        }

        if (pickupSound != null)
        {
            var camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, camPos);
        }

        if (destroyOnCollect) Destroy(gameObject);
    }
}
