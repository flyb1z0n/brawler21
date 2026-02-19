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
        // Track facing direction from movement and rotate gun to match
        if (rb.linearVelocity.x > 0.1f && facingDirection != 1)
        {
            facingDirection = 1;
            UpdateGunTransform();
        }
        else if (rb.linearVelocity.x < -0.1f && facingDirection != -1)
        {
            facingDirection = -1;
            UpdateGunTransform();
        }

        if (equippedGun == null) return;

        if (Input.GetKeyDown(shootKey) && Time.time > lastShotTime + shootCooldown)
        {
            lastShotTime = Time.time;
            Shoot();
        }
    }

    // Flip the gun to the correct side and mirror it to face the right direction
    void UpdateGunTransform()
    {
        if (equippedGun == null) return;

        Transform g = equippedGun.transform;

        // Move gun to whichever side the player faces
        g.localPosition = new Vector3(0.5f * facingDirection, 0f, 0f);

        // Rotate 180° around Y to mirror the sprite so it always points outward
        g.localEulerAngles = new Vector3(0f, facingDirection == 1 ? 0f : 180f, 0f);
    }

    void Shoot()
    {
        Vector2 spawnPos = (Vector2)transform.position + Vector2.right * facingDirection * 0.8f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * facingDirection * bulletSpeed;

        rb.AddForce(Vector2.right * -facingDirection * recoilForce, ForceMode2D.Impulse);
    }

    public void EquipGun(GunPickup gun)
    {
        equippedGun = gun;
        UpdateGunTransform(); // snap to correct orientation immediately on pickup
    }
}
