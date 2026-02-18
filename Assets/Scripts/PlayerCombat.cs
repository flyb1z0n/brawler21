using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Config")]
    public KeyCode punchKey;
    public float punchForce = 18f;
    public float punchRange = 1.2f;
    public float punchCooldown = 0.4f;

    private float lastPunchTime = -999f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(punchKey) && Time.time > lastPunchTime + punchCooldown)
        {
            lastPunchTime = Time.time;
            DoPunch();
        }
    }

    void DoPunch()
    {
        // Detect players in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, punchRange);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.CompareTag("Player")) continue;

            // Direction from this player to the target
            Vector2 dir = (hit.transform.position - transform.position).normalized;

            // Apply force — always push outward + slightly upward
            Vector2 force = (dir + Vector2.up * 0.5f).normalized * punchForce;
            hit.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
            hit.GetComponent<PlayerHealth>()?.TakeDamage(15);
        }
    }

    // Draw punch range in editor for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, punchRange);
    }
}
