using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Moviment : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb2d;
    private Vector2 movement;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

#if ENABLE_INPUT_SYSTEM
    void Update()
    {
        // New Input System-only implementation
        Vector2 input = Vector2.zero;

        // Gamepad (left stick + dpad)
        if (Gamepad.current != null)
        {
            input = Gamepad.current.leftStick.ReadValue();
            if (Gamepad.current.dpad != null)
                input += Gamepad.current.dpad.ReadValue();
        }

        // Keyboard (WASD + arrows)
        if (Keyboard.current != null)
        {
            float right = (Keyboard.current.dKey.isPressed ? 1f : 0f) + (Keyboard.current.rightArrowKey.isPressed ? 1f : 0f);
            float left  = (Keyboard.current.aKey.isPressed ? 1f : 0f) + (Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            float up    = (Keyboard.current.wKey.isPressed ? 1f : 0f) + (Keyboard.current.upArrowKey.isPressed ? 1f : 0f);
            float down  = (Keyboard.current.sKey.isPressed ? 1f : 0f) + (Keyboard.current.downArrowKey.isPressed ? 1f : 0f);

            input += new Vector2(right - left, up - down);
        }

        movement = input;

        if (movement.sqrMagnitude > 1f) movement.Normalize();
    }
#else
    void Update()
    {
        Debug.LogError("Moviment requires the new Input System. Set Player Settings -> Active Input Handling to 'Input System Package (New)' or 'Both' and install the Input System package.");
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
}
