using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Tooltip("Amount of max health to add (and current health).")]
    [SerializeField] private int healthAmount = 1;

    [Tooltip("Duration in seconds. <=0 means permanent.")]
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

        // Find any component in parents that exposes IncreaseMaxHealth(int, float)
        var comps = other.GetComponentsInParent<MonoBehaviour>(true);
        System.Reflection.MethodInfo method = null;
        Component targetComp = null;
        foreach (var c in comps)
        {
            if (c == null) continue;
            method = c.GetType().GetMethod("IncreaseMaxHealth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
            {
                var parms = method.GetParameters();
                if (parms.Length == 2 && parms[0].ParameterType == typeof(int) && parms[1].ParameterType == typeof(float))
                {
                    targetComp = c;
                    break;
                }
            }
        }

        if (method == null || targetComp == null)
        {
            Debug.LogWarning($"HealthPickup: nenhum componente com IncreaseMaxHealth encontrado em '{other.gameObject.name}' ou seus pais.");
            return;
        }

        collected = true;

        method.Invoke(targetComp, new object[] { healthAmount, duration });
        Debug.Log($"HealthPickup: increased max health by {healthAmount} (duration={duration}) on '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (pickupSound != null)
        {
            var camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, camPos);
        }

        if (PickupUI.Instance != null)
        {
            PickupUI.Instance.ShowMessage($"Max Health +{healthAmount}");
        }

        if (destroyOnCollect) Destroy(gameObject);
        collected = false;
    }
}
