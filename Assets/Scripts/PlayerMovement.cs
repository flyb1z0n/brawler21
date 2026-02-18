using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Keybindings")]
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKey(rightKey))
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        else if (Input.GetKey(leftKey))
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Input.GetKeyDown(jumpKey) && isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
