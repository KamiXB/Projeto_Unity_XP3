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
    public void AoReceberLuz(Vector2 posLuz)
    {
        recebendoLuz = true;
        posicaoDaLuz = posLuz;
    }

    public void PararLuz()
    {
        recebendoLuz = false;
    }
}