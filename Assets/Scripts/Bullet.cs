using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float knockbackForce = 14f;

    void Start()
    {
        // 8×8 doodle texture: paper-cream fill with a 1-pixel near-black border.
        // SpriteRenderer.color applies the orange tint: final pixel = paper × orange.
        const int size = 8;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color paper   = new Color(0.97f, 0.95f, 0.90f); // matches DoodleSprite fill
        Color outline = new Color(0.08f, 0.06f, 0.06f); // near-black ink

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool onBorder = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                pixels[y * size + x] = onBorder ? outline : paper;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite.Create(tex, new Rect(0, 0, size, size),
                                        new Vector2(0.5f, 0.5f), size);
        sr.color        = new Color(1.0f, 0.45f, 0.05f); // bright orange marker
        sr.sortingOrder = 3;

        // Apply the shared doodle material so bullets also wobble
        Material doodleMat = Resources.Load<Material>("DoodleMaterial");
        if (doodleMat != null) sr.material = doodleMat;

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
