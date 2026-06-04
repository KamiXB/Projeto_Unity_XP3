using System.Security.Cryptography.X509Certificates;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Moviment : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    // Allow runtime modification via powerups
    private float baseSpeed;

    private Rigidbody2D rb2d;
    private Vector2 movement;
    private Vector2 lastMove;
    private Animator animator;

    [Header("Player")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invulnerabilityTime = 1f;

    private int currentHealth;
    private bool invulnerable = false;
    // Key collection
    private bool hasKey = false;
    public bool HasKey => hasKey;

    public void CollectKey()
    {
        hasKey = true;
        Debug.Log($"Player '{name}' collected the key.");
        // Optional: update animator or UI here
    }

    // Orientação exclusiva
    private bool isFacingFront = true;
    private bool isFacingBack = false;
    private bool isFacingLeft = false;
    private bool isFacingRight = false;
    public bool IsFacingFront => isFacingFront;
    public bool IsFacingBack => isFacingBack;
    public bool IsFacingLeft => isFacingLeft;
    public bool IsFacingRight => isFacingRight;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // Inicializa parâmetros no Animator (opcional)
        SyncAnimatorFacing();

        currentHealth = maxHealth;
        baseSpeed = speed;
    }

    // Increase the player's max health. If duration > 0 the increase is temporary and will be reverted.
    public void IncreaseMaxHealth(int amount, float duration = 0f)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"Moviment: Increased maxHealth by {amount}, new maxHealth={maxHealth}, currentHealth={currentHealth}");

        if (duration > 0f)
        {
            StartCoroutine(MaxHealthDuration(amount, duration));
        }
    }

    private System.Collections.IEnumerator MaxHealthDuration(int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        maxHealth -= amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"Moviment: Reverted maxHealth by {amount}, new maxHealth={maxHealth}");
    }

    // Public API to apply a speed multiplier. duration <= 0 means permanent.
    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f) return;
        speed *= multiplier;

        if (duration > 0f)
        {
            StartCoroutine(SpeedDuration(multiplier, duration));
        }
    }

    private System.Collections.IEnumerator SpeedDuration(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        speed /= multiplier;
    }

#if ENABLE_INPUT_SYSTEM
    void Update()
    {
        Vector2 input = Vector2.zero;

        // Gamepad
        if (Gamepad.current != null)
        {
            input = Gamepad.current.leftStick.ReadValue();
            if (Gamepad.current.dpad != null)
                input += Gamepad.current.dpad.ReadValue();
        }

        // Teclado
        if (Keyboard.current != null)
        {
            float right = (Keyboard.current.dKey.isPressed ? 1f : 0f) + (Keyboard.current.rightArrowKey.isPressed ? 1f : 0f);
            float left = (Keyboard.current.aKey.isPressed ? 1f : 0f) + (Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            float up = (Keyboard.current.wKey.isPressed ? 1f : 0f) + (Keyboard.current.upArrowKey.isPressed ? 1f : 0f);
            float down = (Keyboard.current.sKey.isPressed ? 1f : 0f) + (Keyboard.current.downArrowKey.isPressed ? 1f : 0f);

            input += new Vector2(right - left, up - down);
        }

        // TRAVAR EM 4 DIREÇÕES
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            movement = new Vector2(Mathf.Sign(input.x), 0);
        }
        else if (Mathf.Abs(input.y) > 0)
        {
            movement = new Vector2(0, Mathf.Sign(input.y));
        }
        else
        {
            movement = Vector2.zero;
        }

        // Guarda última direção (para idle)
        if (movement != Vector2.zero)
        {
            lastMove = movement;
            // Atualiza facing de forma exclusiva com base no movimento efetivo
            SetFacingFromDirection(lastMove);
        }

        // ANIMAÇÃO
        if (animator != null)
        {
            animator.SetFloat("moveX", lastMove.x);
            animator.SetFloat("moveY", lastMove.y);
            animator.SetFloat("speed", movement.sqrMagnitude);
            // As booleans de facing já são sincronizadas em SetFacingFromDirection
        }
    }
#else
    void Update()
    {
        Debug.LogError("Moviment requires the new Input System.");
    }
#endif

    void FixedUpdate()
    {
        var delta = movement * speed * Time.fixedDeltaTime;

        if (rb2d != null)
        {
            rb2d.MovePosition(rb2d.position + delta);
        }
        else
        {
            transform.Translate((Vector3)delta, Space.World);
        }
    }

    private void SetFacingFromDirection(Vector2 dir)
    {
        // Torna exclusiva: zera todas e seta apenas a correta
        isFacingFront = isFacingBack = isFacingLeft = isFacingRight = false;

        if (dir.y < 0f)
            isFacingFront = true;
        else if (dir.y > 0f)
            isFacingBack = true;
        else if (dir.x < 0f)
            isFacingLeft = true;
        else if (dir.x > 0f)
            isFacingRight = true;

        SyncAnimatorFacing();
    }

    private void SyncAnimatorFacing()
    {
        if (animator == null) return;
        animator.SetBool("isFacingFront", isFacingFront);
        animator.SetBool("isFacingBack", isFacingBack);
        animator.SetBool("isFacingLeft", isFacingLeft);
        animator.SetBool("isFacingRight", isFacingRight);
    }

    // Public API for damage
    public void TakeDamage(int amount)
    {
        if (invulnerable) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityFlash());
        }
    }

    private void Die()
    {
        // disable movement and shooter if present
        enabled = false;
        var shooter = GetComponent<Shooter>();
        if (shooter != null) shooter.enabled = false;

        if (animator != null) animator.SetTrigger("dead");
        Debug.Log($"Player died: {name}");
    }

    private System.Collections.IEnumerator InvulnerabilityFlash()
    {
        invulnerable = true;
        var sr = GetComponent<SpriteRenderer>();
        float t = 0f;
        while (t < invulnerabilityTime)
        {
            if (sr != null)
            {
                // simple flash
                sr.enabled = (Mathf.FloorToInt(t * 10f) % 2) == 0;
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.enabled = true;
        invulnerable = false;
    }
}