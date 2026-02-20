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
    private const string DoodleMatPath    = "Assets/Resources/DoodleMaterial.mat";
    private const string DoodleShaderName = "Custom/DoodleSprite";

    // Cached sprite — all game objects share the same paper+border texture;
    // the accent colour is set via SpriteRenderer.color (acts as a tint).
    private static Sprite s_DoodleSprite;

    [MenuItem("Brawler/Setup Game Scene")]
    public static void Run()
    {
        s_DoodleSprite = null; // reset cache each run
        EnsureTags();
        EnsureFolders();

        Material doodleMat = EnsureDoodleMaterial();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "GameScene";

        GameObject bulletPrefab = BuildBulletPrefab();

        BuildBackground(doodleMat);

        // Marker palette — applied via SpriteRenderer.color; the sprite texture
        // is paper-cream so: final pixel = paperColour × sr.color.
        Color green  = new Color(0.15f, 0.80f, 0.30f); // green marker
        Color purple = new Color(0.45f, 0.25f, 0.90f); // purple marker

        BuildGround(green, doodleMat);
        BuildPlatform("PlatformLeft",   new Vector2(-5f, -2.0f), new Vector2(4f, 0.3f), purple, doodleMat);
        BuildPlatform("PlatformRight",  new Vector2( 5f, -2.0f), new Vector2(4f, 0.3f), purple, doodleMat);
        BuildPlatform("PlatformCenter", new Vector2( 0f,  0.5f), new Vector2(3f, 0.3f), purple, doodleMat);

        BuildDeathZone();
        BuildGunInScene(new Color(0.98f, 0.82f, 0.02f), doodleMat); // yellow marker

        BuildPlayer(
            playerName:   "Player1",
            color:        new Color(0.10f, 0.40f, 0.95f), // cobalt blue marker
            spawnPos:     new Vector2(-3f, 0f),
            leftKey:      KeyCode.A,
            rightKey:     KeyCode.D,
            jumpKey:      KeyCode.W,
            punchKey:     KeyCode.S,
            shootKey:     KeyCode.LeftShift,
            bulletPrefab: bulletPrefab,
            doodleMat:    doodleMat
        );

        BuildPlayer(
            playerName:   "Player2",
            color:        new Color(0.95f, 0.15f, 0.20f), // red marker
            spawnPos:     new Vector2(3f, 0f),
            leftKey:      KeyCode.LeftArrow,
            rightKey:     KeyCode.RightArrow,
            jumpKey:      KeyCode.UpArrow,
            punchKey:     KeyCode.DownArrow,
            shootKey:     KeyCode.RightControl,
            bulletPrefab: bulletPrefab,
            doodleMat:    doodleMat
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
        if (!AssetDatabase.IsValidFolder("Assets/Shaders"))
            AssetDatabase.CreateFolder("Assets", "Shaders");
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Doodle material — saved to Resources so Bullet.cs can load it at runtime
    // ──────────────────────────────────────────────────────────────────────────

    static Material EnsureDoodleMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(DoodleMatPath);
        if (mat != null) return mat;

        Shader shader = Shader.Find(DoodleShaderName);
        if (shader == null)
        {
            Debug.LogError("[BrawlerSetup] Shader 'Custom/DoodleSprite' not found. " +
                           "Make sure Assets/Shaders/DoodleSprite.shader exists and has compiled.");
            shader = Shader.Find("Sprites/Default");
        }

        mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, DoodleMatPath);
        AssetDatabase.SaveAssets();
        return mat;
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
    // Background — клетчатая тетрадь (graph-paper notebook)
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildBackground(Material doodleMat)
    {
        // Grid dimensions: 60 cols × 44 rows of 16-px cells → 960×704 px texture.
        // At 32 PPU the sprite covers exactly 30 × 22 world units — larger than
        // the camera's visible area (≈25 × 14 wu) so the grid always fills the screen.
        const int cellPx = 16;
        const int cols   = 60;
        const int rows   = 44;
        const int texW   = cols * cellPx; // 960
        const int texH   = rows * cellPx; // 704
        const float ppu  = 32f;           // 1 wu = 2 cells = 32 px

        Texture2D tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color paper  = new Color(0.97f, 0.95f, 0.88f);           // cream base
        Color grid   = new Color(0.60f, 0.78f, 0.95f, 0.70f);   // light-blue grid
        Color margin = new Color(0.95f, 0.45f, 0.45f, 0.85f);   // red margin line

        Color[] pixels = new Color[texW * texH];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = paper;

        // Horizontal grid lines (1 px)
        for (int row = 0; row <= rows; row++)
        {
            int y = row * cellPx;
            if (y >= texH) y = texH - 1;
            for (int x = 0; x < texW; x++)
                pixels[y * texW + x] = grid;
        }

        // Vertical grid lines (1 px)
        for (int col = 0; col <= cols; col++)
        {
            int x = col * cellPx;
            if (x >= texW) x = texW - 1;
            for (int y = 0; y < texH; y++)
                pixels[y * texW + x] = grid;
        }

        // Red margin line — 4 cells from the left edge (2 px wide)
        int mx = 4 * cellPx;
        for (int y = 0; y < texH; y++)
        {
            pixels[y * texW + mx]     = margin;
            pixels[y * texW + mx + 1] = margin;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH),
                                      new Vector2(0.5f, 0.5f), ppu);

        GameObject go = new GameObject("Background");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -10;
        // No wobble on the background — a shimmering grid looks bad.
        // Default sprite material keeps the grid crisp and stable.
        go.transform.position = new Vector3(0f, -1f, 1f);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Ground & platforms
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGround(Color color, Material doodleMat)
    {
        GameObject go = new GameObject("Ground");
        go.tag = "Ground";
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = DoodleSprite();
        sr.color    = color;
        sr.material = doodleMat;
        go.transform.position   = new Vector3(0f, -4f, 0f);
        go.transform.localScale = new Vector3(20f, 0.5f, 1f);
        go.AddComponent<BoxCollider2D>();
    }

    static void BuildPlatform(string name, Vector2 pos, Vector2 scale, Color color, Material doodleMat)
    {
        GameObject go = new GameObject(name);
        go.tag = "Ground";
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = DoodleSprite();
        sr.color    = color;
        sr.material = doodleMat;
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
    // Gun
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildGunInScene(Color color, Material doodleMat)
    {
        GameObject go = new GameObject("Gun");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = DoodleSprite();
        sr.color        = color;
        sr.sortingOrder = 2;
        sr.material     = doodleMat;
        go.transform.localScale = new Vector3(0.5f, 0.2f, 1f);
        go.transform.position   = new Vector3(0f, 0.75f, 0f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

        BoxCollider2D trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;

        go.AddComponent<BoxCollider2D>();

        go.AddComponent<GunPickup>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Players
    // ──────────────────────────────────────────────────────────────────────────

    static void BuildPlayer(
        string playerName, Color color, Vector2 spawnPos,
        KeyCode leftKey, KeyCode rightKey, KeyCode jumpKey,
        KeyCode punchKey, KeyCode shootKey, GameObject bulletPrefab,
        Material doodleMat)
    {
        GameObject go = new GameObject(playerName);
        go.tag = "Player";
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = DoodleSprite();
        sr.color    = color; // PlayerHealth reads this for the floating HP label colour
        sr.material = doodleMat;
        go.transform.localScale = new Vector3(0.6f, 1.0f, 1f);

        Rigidbody2D rb            = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation         = true;

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
        cam.backgroundColor  = new Color(0.97f, 0.95f, 0.88f); // notebook paper cream
        cam.clearFlags       = CameraClearFlags.SolidColor;
        go.AddComponent<AudioListener>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Physics material
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
    // Sprite helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a cached 32×32 doodle sprite: paper-cream interior with a
    /// rough 2-pixel black border (Perlin jitter on the inner edge).
    /// Colour is applied via SpriteRenderer.color (tint multiplier) so one
    /// shared texture drives all differently coloured objects.
    /// </summary>
    static Sprite DoodleSprite()
    {
        if (s_DoodleSprite != null) return s_DoodleSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Paper-cream fill so the SpriteRenderer colour tint produces a
        // "marker on notebook paper" look: final pixel = paperCream × sr.color.
        Color paper   = new Color(0.97f, 0.95f, 0.90f);
        Color outline = new Color(0.08f, 0.06f, 0.06f); // near-black ink

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Perlin jitter makes the inner border edge slightly uneven
                float noiseX = Mathf.PerlinNoise(x * 0.6f, y * 0.6f + 17f);
                float noiseY = Mathf.PerlinNoise(x * 0.6f + 31f, y * 0.6f);
                int jitterX  = noiseX > 0.55f ? 1 : 0;
                int jitterY  = noiseY > 0.55f ? 1 : 0;

                const int borderW = 2;
                bool onBorder =
                    x < borderW + jitterX ||
                    x >= size - borderW - jitterX ||
                    y < borderW + jitterY ||
                    y >= size - borderW - jitterY;

                pixels[y * size + x] = onBorder ? outline : paper;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        s_DoodleSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                       new Vector2(0.5f, 0.5f), size);
        return s_DoodleSprite;
    }

    /// <summary>Plain solid-colour sprite — used only for the background quad.</summary>
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
