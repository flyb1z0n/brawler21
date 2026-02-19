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
    private const string PrefabsPath      = "Assets/Prefabs";
    private const string BulletPrefabPath = "Assets/Prefabs/Bullet.prefab";
    private const string PhysMatPath      = "Assets/PhysicsMaterials/PlayerZeroFriction.asset";

    [MenuItem("Brawler/Setup Game Scene")]
    public static void Run()
    {
        EnsureTags();
        EnsureFolders();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "GameScene";

        GameObject bulletPrefab = BuildBulletPrefab();

        BuildGround();
        BuildPlatform("PlatformLeft",   new Vector2(-5f, -2.0f), new Vector2(4f, 0.3f));
        BuildPlatform("PlatformRight",  new Vector2( 5f, -2.0f), new Vector2(4f, 0.3f));
        BuildPlatform("PlatformCenter", new Vector2( 0f,  0.5f), new Vector2(3f, 0.3f));

        BuildDeathZone();
        BuildGunInScene();

        BuildPlayer(
            playerName:   "Player1",
            color:        Color.blue,
            spawnPos:     new Vector2(-3f, 0f),
            leftKey:      KeyCode.A,
            rightKey:     KeyCode.D,
            jumpKey:      KeyCode.W,
            punchKey:     KeyCode.S,
            shootKey:     KeyCode.LeftShift,
            bulletPrefab: bulletPrefab
        );

        BuildPlayer(
            playerName:   "Player2",
            color:        Color.red,
            spawnPos:     new Vector2(3f, 0f),
            leftKey:      KeyCode.LeftArrow,
            rightKey:     KeyCode.RightArrow,
            jumpKey:      KeyCode.UpArrow,
            punchKey:     KeyCode.DownArrow,
            shootKey:     KeyCode.RightControl,
            bulletPrefab: bulletPrefab
        );

        BuildCamera();

        string scenePath = "Assets/Scenes/GameScene.unity";
        Directory.CreateDirectory(Application.dataPath + "/../Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        // Register in build settings so CI (and File > Build) can find it
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.Refresh();

        Debug.Log("[BrawlerSetup] Scene built and saved to " + scenePath);
        EditorUtility.DisplayDialog("Brawler Setup", "Scene created!\n\n• Open Assets/Scenes/GameScene.unity\n• Press Play to test", "OK");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Tags / Folders
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

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(PrefabsPath))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/PhysicsMaterials"))
            AssetDatabase.CreateFolder("Assets", "PhysicsMaterials");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Bullet prefab — no sprite here; Bullet.cs creates it at runtime
    // ──────────────────────────────────────────────────────────────────────────

    static GameObject BuildBulletPrefab()
    {
        GameObject go = new GameObject("Bullet");
        go.transform.localScale = new Vector3(0.28f, 0.28f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.1f;

        go.AddComponent<Bullet>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, BulletPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Ground & platforms
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGround()
    {
        GameObject go = new GameObject("Ground");
        go.tag = "Ground";
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(new Color(0.3f, 0.6f, 0.3f));
        go.transform.position   = new Vector3(0f, -4f, 0f);
        go.transform.localScale = new Vector3(20f, 0.5f, 1f);
        go.AddComponent<BoxCollider2D>();
    }

    static void BuildPlatform(string name, Vector2 pos, Vector2 scale)
    {
        GameObject go = new GameObject(name);
        go.tag = "Ground";
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(new Color(0.4f, 0.5f, 0.7f));
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
    // Gun — scene object (not prefab) so the sprite serialises into the scene file
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGunInScene()
    {
        GameObject go = new GameObject("Gun");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = SolidSprite(new Color(0.9f, 0.8f, 0.1f)); // bright yellow
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(0.5f, 0.2f, 1f);

        // Center platform top = 1.0 + 0.15 = 1.15; gun half-height = 0.1 → center 1.25
        // Center platform top = 0.5 + 0.15 = 0.65; gun half-height = 0.1 → center at 0.75
        go.transform.position = new Vector3(0f, 0.75f, 0f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 0f; // floats until dropped
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

        BoxCollider2D trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;

        go.AddComponent<BoxCollider2D>(); // solid so it lands after being dropped

        go.AddComponent<GunPickup>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Players
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildPlayer(
        string playerName, Color color, Vector2 spawnPos,
        KeyCode leftKey, KeyCode rightKey, KeyCode jumpKey,
        KeyCode punchKey, KeyCode shootKey, GameObject bulletPrefab)
    {
        GameObject go = new GameObject(playerName);
        go.tag = "Player";
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(color);
        go.transform.localScale = new Vector3(0.6f, 1.0f, 1f);

        Rigidbody2D rb            = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

        // Zero friction prevents the player from hanging on platform edges
        BoxCollider2D playerCol = go.AddComponent<BoxCollider2D>();
        playerCol.sharedMaterial = EnsureZeroFrictionMat();

        PlayerMovement pm = go.AddComponent<PlayerMovement>();
        pm.moveSpeed = 8f;
        pm.jumpForce = 14f;
        pm.leftKey   = leftKey;
        pm.rightKey  = rightKey;
        pm.jumpKey   = jumpKey;

        PlayerCombat pc  = go.AddComponent<PlayerCombat>();
        pc.punchKey      = punchKey;
        pc.punchForce    = 18f;
        pc.punchRange    = 1.2f;
        pc.punchCooldown = 0.4f;

        PlayerRespawn pr = go.AddComponent<PlayerRespawn>();
        pr.spawnPosition = spawnPos;

        PlayerShooting ps  = go.AddComponent<PlayerShooting>();
        ps.shootKey        = shootKey;
        ps.bulletPrefab    = bulletPrefab;
        ps.bulletSpeed     = 20f;
        ps.shootCooldown   = 0.3f;
        ps.recoilForce     = 6f;

        go.AddComponent<PlayerHealth>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Camera
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, -1f, -10f);
        Camera cam           = go.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor  = new Color(0.15f, 0.15f, 0.2f);
        cam.clearFlags       = CameraClearFlags.SolidColor;
        go.AddComponent<AudioListener>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Physics material — zero friction so players don't hang on platform edges
    // ──────────────────────────────────────────────────────────────────────────

    static PhysicsMaterial2D EnsureZeroFrictionMat()
    {
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysMatPath);
        if (mat != null) return mat;

        mat = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(mat, PhysMatPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Sprite helper — bakes color into texture, embedded in the scene file on save
    // ──────────────────────────────────────────────────────────────────────────

    static Sprite SolidSprite(Color color)
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
