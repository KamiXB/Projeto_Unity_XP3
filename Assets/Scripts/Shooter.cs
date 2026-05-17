using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Shooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Firing")]
    [SerializeField] private float fireRate = 4f; // shots per second when holding
    [SerializeField] private int projectilesPerShot = 8; // number spawned around the player
    [SerializeField] private float spawnRadius = 0.5f; // radius around player where bullets originate
    [SerializeField] private bool holdToFire = true; // true = fire while holding button, false = fire on press

#if !ENABLE_INPUT_SYSTEM
    [SerializeField] private KeyCode legacyFireKey = KeyCode.Space; // used if old Input is enabled
#endif

    private float fireCooldown;
    private float baseFireRate;
    [Header("Powerups")]
    [Tooltip("If true bullets will pass through walls. Can be toggled in the inspector or at runtime via SetBulletPenetration().")]
    [SerializeField] private bool bulletsPassThroughWalls = false;

    // Public API to enable penetration for a duration (duration <=0 means permanent)
    public void SetBulletPenetration(bool enabled, float duration = 0f)
    {
        bulletsPassThroughWalls = enabled;
        if (enabled && duration > 0f)
        {
            StartCoroutine(ResetPenetrationAfter(duration));
        }
    }

    private System.Collections.IEnumerator ResetPenetrationAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        bulletsPassThroughWalls = false;
    }

    // Public property so other scripts (e.g. powerup collectors) can set this directly if desired.
    public bool BulletsPassThroughWalls
    {
        get => bulletsPassThroughWalls;
        set => bulletsPassThroughWalls = value;
    }

    void Awake()
    {
        baseFireRate = fireRate;
        // Apply any persistent powerups saved between lives
        if (PlayerPowerups.Instance != null)
        {
            PlayerPowerups.Instance.ApplyToShooter(this);
        }
        // Also apply speed powerups to Moviment if present on same GameObject
        var mov = GetComponent<Moviment>();
        if (mov != null && PlayerPowerups.Instance != null)
        {
            PlayerPowerups.Instance.ApplyToMoviment(mov);
        }
        // Apply light radius powerups to LuzPlayer if present on same GameObject
        var luz = GetComponent<LuzPlayer>();
        if (luz != null && PlayerPowerups.Instance != null)
        {
            PlayerPowerups.Instance.ApplyToLuzPlayer(luz);
        }
    }

    // Public API to modify fire rate (multiplier >1 = faster fire). duration <=0 means permanent
    public void ApplyFireRateMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f) return;
        fireRate *= multiplier;

        if (duration > 0f)
        {
            StartCoroutine(FireRateDuration(multiplier, duration));
        }
    }

    private System.Collections.IEnumerator FireRateDuration(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        fireRate /= multiplier;
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        bool wantFire = false;

#if ENABLE_INPUT_SYSTEM
        // New Input System handling
        var mouse = Mouse.current;
        var gamepad = Gamepad.current;
        var keyboard = Keyboard.current;

        if (holdToFire)
        {
            if (mouse != null && mouse.leftButton.isPressed) wantFire = true;
            if (gamepad != null && gamepad.buttonSouth.isPressed) wantFire = true;
            if (keyboard != null && (keyboard.spaceKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.zKey.isPressed)) wantFire = true;
        }
        else
        {
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) wantFire = true;
            if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) wantFire = true;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame)) wantFire = true;
        }
#else
        // Legacy Input handling (old Input Manager)
        if (holdToFire)
        {
            if (Input.GetMouseButton(0)) wantFire = true;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z)) wantFire = true;
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) wantFire = true;
            if (Input.GetKeyDown(legacyFireKey)) wantFire = true;
        }
#endif

        if (wantFire && fireCooldown <= 0f)
        {
            FireAround();
            fireCooldown = 1f / fireRate;
        }
    }

    private void FireAround()
    {
        if (projectilePrefab == null) return;

        var ownerCollider = GetComponent<Collider2D>();

        float angleStep = 360f / Mathf.Max(1, projectilesPerShot);
        float angle = 0f;

        for (int i = 0; i < projectilesPerShot; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector3 spawnPos = transform.position + (Vector3)(dir * spawnRadius);
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(dir, projectileSpeed, projectileLifetime, ownerCollider, bulletsPassThroughWalls);
            }
            else
            {
                // fallback: move manually
                var rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * projectileSpeed;
                Destroy(proj, projectileLifetime);
            }

            angle += angleStep;
        }
    }
}
