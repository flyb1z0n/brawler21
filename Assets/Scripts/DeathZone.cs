using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Respawn at start position
            col.GetComponent<PlayerRespawn>().Respawn();
        }
    }
}
