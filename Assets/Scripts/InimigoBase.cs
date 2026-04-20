using UnityEngine;

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

    void Update()
    {
        if (player == null) return;

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

        if (distancia <= distanciaDeteccao)
        {
            PerseguirPlayer();
        }
    }

    void PerseguirPlayer()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
    }

    void FugirDaLuz()
    {
        Vector2 direcao = (transform.position - (Vector3)posicaoDaLuz).normalized;
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
    }

    void IrParaLuz()
    {
        Vector2 direcao = (posicaoDaLuz - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
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