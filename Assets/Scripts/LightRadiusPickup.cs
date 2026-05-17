using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightRadiusPickup : MonoBehaviour
{
    [Tooltip("Multiplicador aplicado ao raio da luz do jogador. >1 aumenta o raio.")]
    [SerializeField] private float radiusMultiplier = 1.5f;

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

        // Find any component in parents that exposes ApplyRadiusMultiplier(float, float)
        var comps = other.GetComponentsInParent<MonoBehaviour>(true);
        System.Reflection.MethodInfo method = null;
        Component targetComp = null;
        foreach (var c in comps)
        {
            if (c == null) continue;
            method = c.GetType().GetMethod("ApplyRadiusMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                var parms = method.GetParameters();
                if (parms.Length == 2 && parms[0].ParameterType == typeof(float) && parms[1].ParameterType == typeof(float))
                {
                    targetComp = c;
                    break;
                }
            }
        }

        if (method == null || targetComp == null)
        {
            // If no component found on the player, search the whole scene for any MonoBehaviour with ApplyRadiusMultiplier
            var all = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var a in all)
            {
                if (a == null) continue;
                var m = a.GetType().GetMethod("ApplyRadiusMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m == null) continue;
                var parms = m.GetParameters();
                if (parms.Length == 2 && parms[0].ParameterType == typeof(float) && parms[1].ParameterType == typeof(float))
                {
                    targetComp = a;
                    method = m;
                    break;
                }
            }

            if (method == null || targetComp == null)
            {
                Debug.LogWarning($"LightRadiusPickup: nenhum componente com ApplyRadiusMultiplier encontrado em '{other.gameObject.name}' ou na cena.");
                return;
            }
        }

        collected = true;

        if (duration <= 0f)
        {
            // Save persistent upgrade via reflection (if PlayerPowerups exists)
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
                    var setMethod = ppType.GetMethod("SetLightRadiusUpgrade", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (setMethod != null && instance != null)
                    {
                        setMethod.Invoke(instance, new object[] { radiusMultiplier });
                        Debug.Log($"LightRadiusPickup: saved persistent light radius upgrade x{radiusMultiplier} via PlayerPowerups");
                    }
                }
            }

            method.Invoke(targetComp, new object[] { radiusMultiplier, 0f });
            Debug.Log($"LightRadiusPickup: applied permanent radius x{radiusMultiplier} to '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }
        else
        {
            method.Invoke(targetComp, new object[] { radiusMultiplier, duration });
            Debug.Log($"LightRadiusPickup: applied radius x{radiusMultiplier} for {duration}s to '{targetComp.gameObject.name}' (via {targetComp.GetType().Name})");
        }

        if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
        if (pickupSound != null)
        {
            var camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, camPos);
        }

        if (destroyOnCollect) Destroy(gameObject);
        collected = false;
    }
}
