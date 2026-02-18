using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float knockbackForce = 14f;

    void Start()
    {
        // Sprite is created at runtime to avoid prefab serialization issues.
        // A runtime Texture2D is fine here — bullets are transient objects.
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        sr.color        = new Color(1f, 0.4f, 0f); // bright orange
        sr.sortingOrder = 3;

        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Vector2 dir   = (col.transform.position - transform.position).normalized;
            Vector2 force = (dir + Vector2.up * 0.3f).normalized * knockbackForce;
            col.gameObject.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(25);
        }

        Destroy(gameObject);
    }
}
