using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PortaFase2 : MonoBehaviour
{
    [Header("Detecção")]
    [Tooltip("Tag do jogador que ativa a porta")]
    public string playerTag = "Player";

    [Header("Comportamento")]
    [Tooltip("Se true, a porta só abre quando o jogador possuir a chave (Moviment.HasKey)")]
    public bool requiresKey = true;

    [Header("Animação / Som")]
    [Tooltip("Animator opcional para reproduzir animação de abrir. Deve expor um trigger 'Open' ou bool 'isOpen'.")]
    public Animator animator;
    [Tooltip("Trigger name usado no Animator para abrir (se vazio será usado 'Open')")] 
    public string openTriggerName = "Open";

    [Tooltip("Som reproduzido ao abrir a porta")]
    public AudioClip openSound;
    [Tooltip("Som reproduzido quando o jogador chega perto sem a chave")]
    public AudioClip deniedSound;
    [Tooltip("Se presente, usará este AudioSource para tocar os sons; caso contrário será usado PlayClipAtPoint")]
    public AudioSource audioSource;

    [Header("Opções")]
    [Tooltip("Se true, ao abrir a porta o objeto collider será desativado para permitir passagem")]
    public bool disableColliderOnOpen = true;

    [Header("Detecção Opcional")]
    [Tooltip("Collider2D opcional usado apenas para detectar proximidade do jogador (deve ser Is Trigger). Se vazio, a colisão principal da porta será usada para detectar.")]
    public Collider2D detectionCollider;

    private Collider2D col;
    private bool isOpen = false;

    void Start()
    {
        col = GetComponent<Collider2D>();
        // By default the door collider should block the player until opened.
        if (col != null)
        {
            // do not force trigger on the main collider - keep it blocking
            col.isTrigger = false;
        }

        // If an explicit detection collider was assigned, ensure it's configured as a trigger
        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleProximity(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If detectionCollider is provided (trigger), collisions are not used for detection.
        if (detectionCollider != null) return;

        if (collision == null || collision.collider == null) return;
        HandleProximity(collision.collider);
    }

    private void HandleProximity(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag(playerTag)) return;
        if (isOpen) return;

        var mov = other.GetComponent<Moviment>();
        bool hasKey = mov != null ? mov.HasKey : false;

        if (requiresKey && !hasKey)
        {
            // player near but has no key
            PlayDenied();
            Debug.Log($"PortaFase2: jogador '{other.gameObject.name}' tentou abrir sem chave.");
            return;
        }

        // open
        Debug.Log($"PortaFase2: abrindo porta para '{other.gameObject.name}'");
        Open();
    }

    private void PlayDenied()
    {
        if (deniedSound != null)
        {
            if (audioSource != null) audioSource.PlayOneShot(deniedSound);
            else AudioSource.PlayClipAtPoint(deniedSound, transform.position);
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (animator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            animator.SetTrigger(openTriggerName);
        }

        if (openSound != null)
        {
            if (audioSource != null) audioSource.PlayOneShot(openSound);
            else AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        if (disableColliderOnOpen && col != null)
        {
            col.enabled = false;
        }
    }
}
