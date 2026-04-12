using UnityEngine;

public class InimigoPerseguidor : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;

    [Header("Configuração")]
    public float distanciaDeteccao = 5f;
    public float velocidade = 3f;

    void Update()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= distanciaDeteccao)
        {
            Perseguir();
        }
    }

    void Perseguir()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
        transform.position,
        player.position,
        velocidade * Time.deltaTime
    );
    }
}