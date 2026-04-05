using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool kinematicMove = false;

    [Header("Light Pulse")]
    [SerializeField] private GameObject pulsePrefab;
    [SerializeField] private float pulseInterval = 0.35f;
    [SerializeField] private bool pulseParentToProjectile = true;
    [SerializeField] private bool attachPersistentPulse = true;
    [SerializeField] private float pulseInitialRadius = 0.25f;
    [SerializeField] private float pulseMaxRadius = 3f;
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private Color pulseColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private bool pulseIsPulse = true;

    // Initialize the projectile after instantiation. Optionally pass the owner's collider
    public void Initialize(Vector2 dir, float spd, float life, Collider2D owner = null)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (col != null && owner != null)
        {
            // Ignore collisions between projectile and owner
            var ownerCols = owner.GetComponentsInChildren<Collider2D>();
            foreach (var oc in ownerCols)
            {
                if (oc != null) Physics2D.IgnoreCollision(col, oc, true);
            }
        }

        if (rb != null)
        {
            // Prevent gravity affecting the projectile
            rb.gravityScale = 0f;

            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = direction * speed;
            }
            else
            {
                // Kinematic: move manually in FixedUpdate
                kinematicMove = true;
            }
        }
        else
        {
            // No rigidbody: will move in coroutine
            StartCoroutine(NoRigidbodyMove());
        }

        // Attach a persistent pulse to this projectile if requested
        if (pulsePrefab != null && attachPersistentPulse)
        {
            GameObject go = Instantiate(pulsePrefab, transform.position, Quaternion.identity);
            if (go.scene.rootCount == 0)
            {
                go = Instantiate(pulsePrefab);
                go.transform.position = transform.position;
            }
            if (!go.activeSelf) go.SetActive(true);
            // parent and zero local position so it stays on projectile
            if (pulseParentToProjectile)
            {
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
            }
            var lp = go.GetComponent<LightPulse>();
            if (lp != null)
            {
                lp.initialRadius = pulseInitialRadius;
                lp.maxRadius = pulseMaxRadius;
                lp.duration = pulseDuration;
                lp.color = pulseColor;
                // persistent attached pulse should not auto-destroy, so disable single pulse behavior
                lp.pulse = false;
            }
        }

        // Start spawning periodic light pulses if configured and not attaching persistent one
        if (!attachPersistentPulse && pulsePrefab != null && pulseInterval > 0f)
        {
            StartCoroutine(SpawnPulses());
        }

        Destroy(gameObject, lifetime);
    }

    private IEnumerator NoRigidbodyMove()
    {
        float t = 0f;
        while (t < lifetime)
        {
            transform.Translate((Vector3)direction * speed * Time.deltaTime, Space.World);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (kinematicMove && rb != null)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }

    private IEnumerator SpawnPulses()
    {
        while (true)
        {
            if (pulsePrefab != null)
            {
                Vector3 pos = transform.position;
                GameObject go = Instantiate(pulsePrefab, pos, Quaternion.identity);

                // Defensive: if Instantiate returned the prefab asset (rare), create a proper runtime instance
                // Prefab assets have no valid scene (rootCount == 0), runtime instances belong to a scene.
                if (go.scene.rootCount == 0)
                {
                    Debug.LogWarning("Pulse instantiation returned an asset instead of a scene instance — creating proper instance.");
                    go = Instantiate(pulsePrefab);
                    go.transform.position = pos;
                }

                // Ensure active
                if (!go.activeSelf) go.SetActive(true);

                // Optionally parent to projectile so pulse follows during its lifetime
                if (pulseParentToProjectile)
                {
                    // parent without preserving world position so localPosition stays zero
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = Vector3.zero;
                }

                var lp = go.GetComponent<LightPulse>();
                if (lp != null)
                {
                    lp.initialRadius = pulseInitialRadius;
                    lp.maxRadius = pulseMaxRadius;
                    lp.duration = pulseDuration;
                    lp.color = pulseColor;
                    lp.pulse = pulseIsPulse;
                }
            }

            yield return new WaitForSeconds(pulseInterval);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy on first collision
        Destroy(gameObject);
    }
}
