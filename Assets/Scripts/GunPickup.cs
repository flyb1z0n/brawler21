using UnityEngine;

public class GunPickup : MonoBehaviour
{
    private bool isHeld = false;
    private GameObject holder = null;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isHeld) return;
        if (!col.CompareTag("Player")) return;

        PickUp(col.gameObject);
    }

    void PickUp(GameObject player)
    {
        isHeld = true;
        holder = player;

        // Attach to player
        transform.SetParent(player.transform);
        transform.localPosition = new Vector2(0.5f, 0f);

        GetComponent<Rigidbody2D>().simulated = false;
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        // Give player the ability to shoot
        player.GetComponent<PlayerShooting>().EquipGun(this);
    }

    public void Drop()
    {
        isHeld = false;
        holder = null;

        transform.SetParent(null);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f; // restore gravity so the gun falls after being thrown
        rb.simulated = true;
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = true;

        // Toss the gun in the direction the player was facing
        rb.AddForce(Vector2.right * Random.Range(-3f, 3f) + Vector2.up * 3f, ForceMode2D.Impulse);
    }
}
