using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector2 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
