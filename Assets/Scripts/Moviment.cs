using System.Security.Cryptography.X509Certificates;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Moviment : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb2d;
    private Vector2 movement;
    private Vector2 lastMove;
    private Animator animator;

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
}