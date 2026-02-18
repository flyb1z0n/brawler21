using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Run via menu: Brawler > Setup Game Scene
/// Creates all GameObjects, prefabs, and scene layout for the Phase 1 prototype.
/// Safe to re-run — it clears and rebuilds the scene each time.
/// </summary>
public static class SetupScene
{
    private const string PrefabsPath = "Assets/Prefabs";
    private const string BulletPrefabPath = "Assets/Prefabs/Bullet.prefab";
    private const string GunPrefabPath = "Assets/Prefabs/Gun.prefab";

    [MenuItem("Brawler/Setup Game Scene")]
    public static void Run()
    {
        EnsureTags();
        EnsureFolders();

        // Create / wipe the scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "GameScene";

        // Build prefabs first (needed by PlayerShooting reference)
        GameObject bulletPrefab = BuildBulletPrefab();
        BuildGunPrefab();

        // World geometry
        BuildGround();
        BuildPlatform("PlatformLeft",  new Vector2(-5f, -1.5f), new Vector2(4f, 0.3f));
        BuildPlatform("PlatformRight", new Vector2( 5f, -1.5f), new Vector2(4f, 0.3f));
        BuildPlatform("PlatformCenter",new Vector2( 0f,  1.0f), new Vector2(3f, 0.3f));

        // Death zone
        BuildDeathZone();

        // Gun world instance
        BuildGunInstance();

        // Players
        BuildPlayer(
            playerName:    "Player1",
            color:         Color.blue,
            spawnPos:      new Vector2(-3f, 0f),
            leftKey:       KeyCode.A,
            rightKey:      KeyCode.D,
            jumpKey:       KeyCode.W,
            punchKey:      KeyCode.S,
            shootKey:      KeyCode.F,
            bulletPrefab:  bulletPrefab
        );

        BuildPlayer(
            playerName:    "Player2",
            color:         Color.red,
            spawnPos:      new Vector2(3f, 0f),
            leftKey:       KeyCode.LeftArrow,
            rightKey:      KeyCode.RightArrow,
            jumpKey:       KeyCode.UpArrow,
            punchKey:      KeyCode.DownArrow,
            shootKey:      KeyCode.RightControl,
            bulletPrefab:  bulletPrefab
        );

        // Camera
        BuildCamera();

        // Save scene
        string scenePath = "Assets/Scenes/GameScene.unity";
        Directory.CreateDirectory(Application.dataPath + "/../Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[BrawlerSetup] Scene built and saved to " + scenePath);
        EditorUtility.DisplayDialog("Brawler Setup", "Scene created!\n\n• Open Assets/Scenes/GameScene.unity\n• Press Play to test", "OK");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Tags
    // ──────────────────────────────────────────────────────────────────────────

    static void EnsureTags()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        EnsureTag(tagsProp, "Player");
        EnsureTag(tagsProp, "Ground");
        EnsureTag(tagsProp, "DeathZone");

        tagManager.ApplyModifiedProperties();
    }

    static void EnsureTag(SerializedProperty tagsProp, string tag)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.arraySize++;
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Folders
    // ──────────────────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(PrefabsPath))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Bullet prefab
    // ──────────────────────────────────────────────────────────────────────────

    static GameObject BuildBulletPrefab()
    {
        GameObject go = new GameObject("Bullet");

        // Visual — tiny yellow circle approximated with a square sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = CreateSolidSquareSprite(Color.yellow, 16);
        sr.drawMode = SpriteDrawMode.Simple;
        go.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        // Physics
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale       = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation     = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.1f;

        go.AddComponent<Bullet>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, BulletPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Gun prefab
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGunPrefab()
    {
        GameObject go = new GameObject("Gun");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSquareSprite(new Color(0.6f, 0.4f, 0.1f), 32);
        go.transform.localScale = new Vector3(0.4f, 0.2f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        go.AddComponent<GunPickup>();

        PrefabUtility.SaveAsPrefabAsset(go, GunPrefabPath);
        Object.DestroyImmediate(go);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Ground & platforms
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGround()
    {
        GameObject go = new GameObject("Ground");
        go.tag = "Ground";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSquareSprite(new Color(0.3f, 0.6f, 0.3f), 64);
        go.transform.position   = new Vector3(0f, -4f, 0f);
        go.transform.localScale = new Vector3(20f, 0.5f, 1f);

        go.AddComponent<BoxCollider2D>();
    }

    static void BuildPlatform(string name, Vector2 pos, Vector2 scale)
    {
        GameObject go = new GameObject(name);
        go.tag = "Ground";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSquareSprite(new Color(0.4f, 0.5f, 0.7f), 64);
        go.transform.position   = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        go.AddComponent<BoxCollider2D>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Death zone
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildDeathZone()
    {
        GameObject go = new GameObject("DeathZone");
        go.tag = "DeathZone";
        go.transform.position = new Vector3(0f, -7f, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(30f, 1f);

        go.AddComponent<DeathZone>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Gun world instance
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGunInstance()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[BrawlerSetup] Gun prefab not found at " + GunPrefabPath);
            return;
        }
        GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(0f, 0.5f, 0f);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Players
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildPlayer(
        string playerName,
        Color color,
        Vector2 spawnPos,
        KeyCode leftKey, KeyCode rightKey, KeyCode jumpKey,
        KeyCode punchKey,
        KeyCode shootKey,
        GameObject bulletPrefab)
    {
        GameObject go = new GameObject(playerName);
        go.tag = "Player";
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

        // Visual
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSquareSprite(color, 64);
        go.transform.localScale = new Vector3(0.6f, 1.0f, 1f);

        // Physics
        Rigidbody2D rb             = go.AddComponent<Rigidbody2D>();
        rb.gravityScale            = 3f;
        rb.collisionDetectionMode  = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation          = true;

        go.AddComponent<BoxCollider2D>();

        // Scripts — Movement
        PlayerMovement pm = go.AddComponent<PlayerMovement>();
        pm.moveSpeed  = 8f;
        pm.jumpForce  = 12f;
        pm.leftKey    = leftKey;
        pm.rightKey   = rightKey;
        pm.jumpKey    = jumpKey;

        // Scripts — Combat
        PlayerCombat pc = go.AddComponent<PlayerCombat>();
        pc.punchKey      = punchKey;
        pc.punchForce    = 18f;
        pc.punchRange    = 1.2f;
        pc.punchCooldown = 0.4f;

        // Scripts — Respawn
        PlayerRespawn pr = go.AddComponent<PlayerRespawn>();
        pr.spawnPosition = spawnPos;

        // Scripts — Shooting
        PlayerShooting ps = go.AddComponent<PlayerShooting>();
        ps.shootKey     = shootKey;
        ps.bulletPrefab = bulletPrefab;
        ps.bulletSpeed  = 20f;
        ps.shootCooldown = 0.3f;
        ps.recoilForce  = 6f;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Camera
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, -1f, -10f);

        Camera cam        = go.AddComponent<Camera>();
        cam.orthographic  = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor  = new Color(0.15f, 0.15f, 0.2f);
        cam.clearFlags    = CameraClearFlags.SolidColor;

        go.AddComponent<AudioListener>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Sprite helper — creates a 1×1 white square texture tinted to a color
    // ──────────────────────────────────────────────────────────────────────────

    static Sprite CreateSolidSquareSprite(Color color, int texSize = 64)
    {
        Texture2D tex = new Texture2D(texSize, texSize);
        Color[] pixels = new Color[texSize * texSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, texSize, texSize),
            new Vector2(0.5f, 0.5f),
            texSize   // pixels per unit = texSize so sprite is exactly 1 Unity unit
        );
    }
}
