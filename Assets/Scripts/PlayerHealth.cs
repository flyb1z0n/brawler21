using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;

    private int currentHealth;
    private TextMesh hpText;
    private Transform labelRoot;

    void Awake()
    {
        currentHealth = maxHealth;

        // Floating label as a separate root object — avoids player scale distortion
        GameObject label = new GameObject("HpLabel_" + gameObject.name);
        labelRoot = label.transform;

        hpText = label.AddComponent<TextMesh>();
        hpText.alignment = TextAlignment.Center;
        hpText.anchor = TextAnchor.LowerCenter;
        hpText.fontSize = 40;
        hpText.characterSize = 0.1f;

        // Tint label to match player colour so it's clear who is who
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        hpText.color = sr != null ? sr.color : Color.white;

        // Render above all sprites
        label.GetComponent<MeshRenderer>().sortingOrder = 10;

        UpdateText();
    }

    void LateUpdate()
    {
        if (labelRoot != null)
            labelRoot.position = transform.position + Vector3.up * 0.9f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateText();
        if (currentHealth <= 0)
            GetComponent<PlayerRespawn>().Respawn();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateText();
    }

    void UpdateText()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHealth;
    }

    void OnDestroy()
    {
        if (labelRoot != null)
            Destroy(labelRoot.gameObject);
    }
}
