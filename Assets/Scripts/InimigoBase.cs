using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InimigoBase : MonoBehaviour
{
    public Transform player;

    [Header("Stats")]
    public float velocidade = 3f;
    public float distanciaDeteccao = 5f;

    [Header("Estados")]
    public bool ativo = true;
    public bool afetadoPelaLuz = false;
    public bool comMedoDaLuz = false;
    public bool paraComLuz = false;

    private bool recebendoLuz = false;
    private Vector2 posicaoDaLuz;
    [Header("Debug")]
    [SerializeField] private bool logLightEvents = true;

    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Combate")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackCooldown = 1f;
    private float attackTimer = 0f;
    private Moviment playerMovRef;

    [Header("Physics Movement")]
    [SerializeField] private float skinWidth = 0.02f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (rb == null)
        {
            Debug.LogWarning($"Inimigo '{name}' requires a Rigidbody2D for physics movement.");
        }
        if (col == null)
        {
            Debug.LogWarning($"Inimigo '{name}' should have a Collider2D to interact with walls.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // attack cooldown timer
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        // 🔦 SE ESTÁ NA LUZ
        if (recebendoLuz)
        {
            if (comMedoDaLuz)
            {
                FugirDaLuz();
                return;
            }

            if (paraComLuz)
            {
                return;
            }

            if (afetadoPelaLuz)
            {
                IrParaLuz();
                return;
            }
        }

        // 🎯 comportamento normal
        if (!ativo) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        // If close enough, attack instead of chasing
        if (distancia <= attackRange)
        {
            TryAttackPlayer();
            return;
        }

        if (distancia <= distanciaDeteccao)
        {
            PerseguirPlayer();
        }
    }

    private void TryAttackPlayer()
    {
        if (attackTimer > 0f) return;

        // resolve Moviment (player) if not cached
        if (playerMovRef == null && player != null)
        {
            playerMovRef = player.GetComponent<Moviment>();
        }

        if (playerMovRef != null)
        {
            playerMovRef.TakeDamage(attackDamage);
            if (logLightEvents) Debug.Log($"Inimigo '{name}' atacou o jogador for {attackDamage} de dano");
        }
        else if (player != null)
        {
            // fallback: try SendMessage so user can implement method with any name
            player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
            if (logLightEvents) Debug.Log($"Inimigo '{name}' atacou jogador via SendMessage ({attackDamage})");
        }

        attackTimer = attackCooldown;
    }

    void PerseguirPlayer()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        MoveWithPhysics(direcao * velocidade * Time.deltaTime);
    }

    void FugirDaLuz()
    {
        Vector2 direcao = (transform.position - (Vector3)posicaoDaLuz).normalized;
        MoveWithPhysics(direcao * velocidade * Time.deltaTime);
    }

    void IrParaLuz()
    {
        Vector2 direcao = (posicaoDaLuz - (Vector2)transform.position).normalized;
        MoveWithPhysics(direcao * velocidade * Time.deltaTime);
    }

    // Try to move using Rigidbody2D and respect collisions. If blocked, try sliding.
    private void MoveWithPhysics(Vector2 displacement)
    {
        if (rb == null)
        {
            // fallback to transform if no rigidbody
            transform.position += (Vector3)displacement;
            return;
        }

        if (displacement.sqrMagnitude < 0.000001f) return;

        Vector2 dir = displacement.normalized;
        float dist = displacement.magnitude;

        RaycastHit2D[] hits = new RaycastHit2D[8];
        int hitCount = rb.Cast(dir, hits, dist + skinWidth);

        if (hitCount == 0)
        {
            rb.MovePosition(rb.position + displacement);
            return;
        }

        // compute average normal
        Vector2 avgNormal = Vector2.zero;
        for (int i = 0; i < hitCount; i++) avgNormal += hits[i].normal;
        avgNormal /= hitCount;
        avgNormal.Normalize();

        // slide along surface: remove normal component
        Vector2 slide = displacement - Vector2.Dot(displacement, avgNormal) * avgNormal;
        if (slide.sqrMagnitude > 0.0001f)
        {
            RaycastHit2D[] slideHits = new RaycastHit2D[8];
            int sCount = rb.Cast(slide.normalized, slideHits, slide.magnitude + skinWidth);
            if (sCount == 0)
            {
                rb.MovePosition(rb.position + slide);
                return;
            }
        }

        // if cannot slide, try small perpendicular offsets
        Vector2 perp = Vector2.Perpendicular(dir).normalized * (dist * 0.5f);
        RaycastHit2D[] pHits = new RaycastHit2D[8];
        int pCount = rb.Cast(perp.normalized, pHits, perp.magnitude + skinWidth);
        if (pCount == 0)
        {
            rb.MovePosition(rb.position + perp);
            return;
        }

        perp = -perp;
        pCount = rb.Cast(perp.normalized, pHits, perp.magnitude + skinWidth);
        if (pCount == 0)
        {
            rb.MovePosition(rb.position + perp);
            return;
        }

        // blocked: do not move this frame
    }

    // 🔦 chamado pela luz
    // Called by light with position only (keeps previous behavior)
    public void AoReceberLuz(Vector2 posLuz)
    {
        // default: treat as inside light
        AoReceberLuz(posLuz, float.PositiveInfinity);
    }

    // Called by light with position and radius: will only mark as receiving light if inside radius
    public void AoReceberLuz(Vector2 posLuz, float radius)
    {
        posicaoDaLuz = posLuz;
        float distSqr = ((Vector2)transform.position - posLuz).sqrMagnitude;
        bool inside = distSqr <= radius * radius;

        if (inside)
        {
            if (!recebendoLuz && logLightEvents) Debug.Log($"Inimigo '{name}' entrou no raio da luz. dist={Mathf.Sqrt(distSqr):F2} radius={radius:F2}");
            recebendoLuz = true;
        }
        else
        {
            if (recebendoLuz && logLightEvents) Debug.Log($"Inimigo '{name}' saiu do raio da luz. dist={Mathf.Sqrt(distSqr):F2} radius={radius:F2}");
            recebendoLuz = false;
        }
    }

    public void PararLuz()
    {
        if (recebendoLuz && logLightEvents) Debug.Log($"Inimigo '{name}' PararLuz() called - no longer receiving light.");
        recebendoLuz = false;
    }
}