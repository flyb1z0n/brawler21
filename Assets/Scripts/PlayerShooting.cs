using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Config")]
    public KeyCode shootKey;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float shootCooldown = 0.3f;
    public float recoilForce = 6f;

    private GunPickup equippedGun = null;
    private float lastShotTime = -999f;
    private Rigidbody2D rb;
    private int facingDirection = 1; // 1 = right, -1 = left

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Track facing direction from movement
        if (rb.linearVelocity.x > 0.1f) facingDirection = 1;
        else if (rb.linearVelocity.x < -0.1f) facingDirection = -1;

        if (equippedGun == null) return;

        if (Input.GetKeyDown(shootKey) && Time.time > lastShotTime + shootCooldown)
        {
            lastShotTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        // Spawn bullet
        Vector2 spawnPos = (Vector2)transform.position + Vector2.right * facingDirection * 0.8f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * facingDirection * bulletSpeed;

        // Recoil pushes shooter backward
        rb.AddForce(Vector2.right * -facingDirection * recoilForce, ForceMode2D.Impulse);
    }

    public void EquipGun(GunPickup gun)
    {
        equippedGun = gun;
    }
}
