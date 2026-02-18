using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float knockbackForce = 14f;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Vector2 dir = (col.transform.position - transform.position).normalized;
            Vector2 force = (dir + Vector2.up * 0.3f).normalized * knockbackForce;
            col.gameObject.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        Destroy(gameObject);
    }

    void Start()
    {
        // Auto-destroy bullet after 3 seconds
        Destroy(gameObject, 3f);
    }
}
