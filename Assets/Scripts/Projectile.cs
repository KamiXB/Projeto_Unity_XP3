using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;

    private Rigidbody2D rb;

    // Initialize the projectile after instantiation
    public void Initialize(Vector2 dir, float spd, float life)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }

    // Fallback if someone forgets to call Initialize: move forward and self-destruct
    IEnumerator Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            // no rigidbody: move manually until destroyed
            float t = 0f;
            while (t < lifetime)
            {
                transform.Translate((Vector3)direction * speed * Time.deltaTime, Space.World);
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
        else
        {
            // Rigidbody was set by Initialize, nothing else needed
            yield break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Optionally ignore collisions with the shooter; handle damage here
        // Destroy on first collision
        Destroy(gameObject);
    }
}
