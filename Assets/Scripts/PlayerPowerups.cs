using UnityEngine;

// Singleton that stores permanent powerups that should persist across deaths/scene loads.
public class PlayerPowerups : MonoBehaviour
{
    private static PlayerPowerups instance;

    public static PlayerPowerups Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerPowerups>();
                if (instance == null)
                {
                    var go = new GameObject("_PlayerPowerups");
                    instance = go.AddComponent<PlayerPowerups>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    [Header("Persistent Powerups")]
    [SerializeField] private bool hasFireRateUpgrade = false;
    [SerializeField] private float fireRateMultiplier = 1f;

    public bool HasFireRateUpgrade => hasFireRateUpgrade;
    public float FireRateMultiplier => fireRateMultiplier;

    public void SetFireRateUpgrade(float multiplier)
    {
        hasFireRateUpgrade = true;
        fireRateMultiplier = multiplier;
        Debug.Log($"PlayerPowerups: fire rate upgrade saved (x{multiplier})");
    }

    public void ApplyToShooter(Component shooterComponent)
    {
        if (shooterComponent == null) return;
        if (!hasFireRateUpgrade || fireRateMultiplier == 1f) return;

        var method = shooterComponent.GetType().GetMethod("ApplyFireRateMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(shooterComponent, new object[] { fireRateMultiplier, 0f });
            Debug.Log($"PlayerPowerups: applied persistent fire rate x{fireRateMultiplier} to '{shooterComponent.gameObject.name}' (via {shooterComponent.GetType().Name})");
        }
    }
}
