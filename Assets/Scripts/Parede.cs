using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Parede : MonoBehaviour
{
    [Tooltip("Layers that the wall will react to (projectiles, players, etc).")]
    [SerializeField] private LayerMask collisionMask = ~0;

    [Tooltip("If true, projectiles that hit the wall will be destroyed.")]
    [SerializeField] private bool destroyProjectiles = true;

    [Header("Optional Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float effectDuration = 1f;

    private void OnValidate()
    {
        // Ensure there's a Collider2D
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"Parede component on '{name}' should be attached to a GameObject with a Collider2D.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Log collision details for debugging
        if (collision != null && collision.collider != null)
        {
            var contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)collision.collider.transform.position;
            Debug.Log($"Parede collided with '{collision.gameObject.name}' at {contactPoint} (layer={collision.gameObject.layer}, tag={collision.gameObject.tag})");
        }

        HandleCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            Debug.Log($"Parede trigger entered by '{other.gameObject.name}' at {other.transform.position} (layer={other.gameObject.layer}, tag={other.gameObject.tag})");
        }

        HandleCollision(other);
    }

    private void HandleCollision(Collider2D other)
    {
        if (other == null) return;

        // Only react to layers included in the mask
        if (((1 << other.gameObject.layer) & collisionMask) == 0) return;

        // Debug log details about the collider that hit the wall
        var proj = other.GetComponentInParent<Projectile>();
        bool isProjectile = proj != null;
        Debug.Log($"Parede.HandleCollision: hit by '{other.gameObject.name}' (isProjectile={isProjectile}) on layer {other.gameObject.layer} tag '{other.gameObject.tag}'");

        // If it's a projectile, destroy it (or let projectile handle it)
        if (isProjectile)
        {
            // If projectile can pass through walls, ignore it
            if (proj.CanPassThroughWalls)
            {
                Debug.Log("Parede: projectile can pass through walls, ignoring.");
                return;
            }

            if (destroyProjectiles)
            {
                Destroy(proj.gameObject);
            }
            SpawnHitEffect(other.transform.position);
            return;
        }

        // Characters and physics objects: physics engine will handle blocking.
        // You can add additional responses here (damage, sound, bounce, etc.).
        SpawnHitEffect(other.transform.position);
    }

    private void SpawnHitEffect(Vector2 position)
    {
        if (hitEffectPrefab != null)
        {
            var go = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            if (effectDuration > 0f) Destroy(go, effectDuration);
        }

        if (hitSound != null)
        {
            var camPos = Camera.main != null ? Camera.main.transform.position : (Vector3)position;
            AudioSource.PlayClipAtPoint(hitSound, camPos);
        }
    }
}
