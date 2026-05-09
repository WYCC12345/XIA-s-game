using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RubricGameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildGameIfNeeded()
    {
        if (FindObjectOfType<RubricGameBootstrap>() != null)
        {
            return;
        }

        GameObject runner = new GameObject("Rubric Game Bootstrap");
        runner.AddComponent<RubricGameBootstrap>();
    }

    private enum GameState
    {
        Menu,
        Instructions,
        Changes,
        Playing,
        Paused,
        Win,
        GameOver
    }

    private const float ArenaHalfWidth = 8.6f;
    private const float ArenaHalfHeight = 4.7f;
    private const float MenuContentOffsetX = 230f;
    private const float MenuTitleExtraOffsetX = 70f;
    private const float MenuButtonOffsetY = 45f;

    private readonly List<EnemyShip> enemies = new List<EnemyShip>();
    private readonly List<PlayerShot> playerShots = new List<PlayerShot>();
    private readonly List<EnemyShot> enemyShots = new List<EnemyShot>();
    private readonly List<AsteroidObstacle> asteroids = new List<AsteroidObstacle>();
    private readonly List<PowerUpPickup> powerUps = new List<PowerUpPickup>();

    private GameState state = GameState.Menu;
    private Canvas canvas;
    private RectTransform root;
    private RectTransform hudPanel;
    private RectTransform centerPanel;
    private Text scoreText;
    private Text statusText;
    private Text feedbackText;
    private Text objectiveText;
    private Camera mainCamera;
    private PlayerShip player;
    private Transform worldRoot;
    private Transform shotRoot;
    private int score;
    private int highScore;
    private int wave;
    private int lives;
    private float playTimer;
    private float enemySpawnTimer;
    private float powerUpSpawnTimer;
    private float feedbackTimer;
    private float shakeTimer;
    private Vector3 cameraBasePosition;
    private LevelDefinition[] levels;
    private int selectedLevelIndex;

    private Sprite playerSprite;
    private Sprite enemySprite;
    private Sprite asteroidSprite;
    private Sprite shotSprite;
    private Sprite enemyShotSprite;
    private Sprite powerUpSprite;
    private Sprite buttonSprite;
    private Sprite[] playerSprites;
    private Sprite[] enemySprites;
    private Sprite[] asteroidSprites;
    private Sprite[] backgroundSprites;
    private Sprite[] planetSprites;
    private Sprite[] stationSprites;
    private Sprite[] blackHoleSprites;
    private GameObject playerHitEffect;
    private GameObject playerDeathEffect;
    private GameObject enemyHitEffect;
    private GameObject enemyDeathEffect;
    private GameObject projectileHitEffect;
    private GameObject victoryEffect;
    private GameObject gameOverEffect;

    private LevelDefinition CurrentLevel
    {
        get { return levels[Mathf.Clamp(selectedLevelIndex, 0, levels.Length - 1)]; }
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        highScore = PlayerPrefs.GetInt("RubricHighScore", 0);
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
        else if (mainCamera.GetComponent<AudioListener>() == null)
        {
            mainCamera.gameObject.AddComponent<AudioListener>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5.3f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
        mainCamera.transform.position = new Vector3(0, 0, -10);
        cameraBasePosition = mainCamera.transform.position;

        worldRoot = new GameObject("Runtime Game World").transform;
        shotRoot = new GameObject("Projectile Holder").transform;
        shotRoot.SetParent(worldRoot);

        CreateSprites();
        CreateLevels();
        CreateBackground();
        CreateCanvas();
        ShowMenu();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (state == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (state == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }

        if (state == GameState.Playing)
        {
            UpdatePlaying();
        }

        UpdateFeedback();
        UpdateCameraShake();
    }

    private void CreateSprites()
    {
        playerSprite = MakeTriangleSprite(new Color(0.20f, 0.85f, 1f), new Color(1f, 1f, 1f));
        enemySprite = MakeTriangleSprite(new Color(1f, 0.30f, 0.24f), new Color(1f, 0.78f, 0.20f));
        asteroidSprite = MakeCircleSprite(new Color(0.56f, 0.55f, 0.62f), new Color(0.26f, 0.27f, 0.34f));
        shotSprite = MakeCircleSprite(new Color(1f, 0.91f, 0.24f), new Color(1f, 0.47f, 0.12f));
        enemyShotSprite = MakeCircleSprite(new Color(1f, 0.32f, 0.22f), new Color(1f, 0.72f, 0.15f));
        powerUpSprite = MakeDiamondSprite(new Color(0.32f, 1f, 0.62f), new Color(0.15f, 0.50f, 1f));

#if UNITY_EDITOR
        playerSprites = LoadSpritesAtPath("Assets/Art/Player/Player Sprites.png");
        enemySprites = LoadSpritesAtPath("Assets/Art/Enemies/Straight Shooter/SShoot Sprites.png");
        asteroidSprites = LoadSpriteList(
            "Assets/Art/Environment/Asteroids/Asteroid_1.png",
            "Assets/Art/Environment/Asteroids/Asteroid_2.png",
            "Assets/Art/Environment/Asteroids/Asteroid_3.png",
            "Assets/Art/Environment/Asteroids/Asteroid_4.png");
        backgroundSprites = LoadSpriteList(
            "Assets/Art/Environment/Background/A_CompleteSpaceBackground.png",
            "Assets/Art/Environment/Background/B_CompleteSpaceBackground.png",
            "Assets/Art/Environment/Background/C_CompleteSpaceBackground.png",
            "Assets/Art/Environment/Background/D_CompleteSpaceBackground.png");
        planetSprites = LoadSpriteList(
            "Assets/Art/Environment/Planets/Big/BigBluePlanet.png",
            "Assets/Art/Environment/Planets/Big/BigRedPlanet.png",
            "Assets/Art/Environment/Planets/Medium/Medium_RingedRedPlanet.png",
            "Assets/Art/Environment/Planets/Small/Small_RingedYellowPlanet.png");
        stationSprites = LoadSpriteList(
            "Assets/Art/Environment/Space_Stations/SolarSpaceStation.png",
            "Assets/Art/Environment/Space_Stations/AlienSpaceStation.png",
            "Assets/Art/Environment/Space_Stations/PyramidSpaceStation.png",
            "Assets/Art/Environment/Space_Stations/Small_SpaceStation.png");
        blackHoleSprites = LoadSpriteList(
            "Assets/Art/Environment/Black_Holes/Small_Blackhole.png",
            "Assets/Art/Environment/Black_Holes/MoreAccurate_Blackhole.png",
            "Assets/Art/Environment/Black_Holes/Big_Blackhole.png");

        Sprite[] playerShotSprites = LoadSpritesAtPath("Assets/Art/Projectiles/Player Projectile/Player_Projectile.png");
        if (playerShotSprites.Length > 0)
        {
            shotSprite = playerShotSprites[0];
        }

        Sprite[] enemyProjectileSprites = LoadSpritesAtPath("Assets/Art/Projectiles/Enemy Projectiles/Straight Projectile/Enemy_StraightProjectile.png");
        if (enemyProjectileSprites.Length > 0)
        {
            enemyShotSprite = enemyProjectileSprites[0];
        }

        if (playerSprites.Length > 0)
        {
            playerSprite = playerSprites[0];
        }
        if (enemySprites.Length > 0)
        {
            enemySprite = enemySprites[0];
        }
        if (asteroidSprites.Length > 0)
        {
            asteroidSprite = asteroidSprites[0];
        }

        Sprite[] buttonSprites = LoadSpriteList("Assets/Art/UI Elements/Buttons/UIButton.png");
        if (buttonSprites.Length > 0)
        {
            buttonSprite = buttonSprites[0];
        }

        playerHitEffect = LoadPrefab("Assets/Prefabs/Effects/Player/PlayerHitEffect.prefab");
        playerDeathEffect = LoadPrefab("Assets/Prefabs/Effects/Player/PlayerDeathEffect.prefab");
        enemyHitEffect = LoadPrefab("Assets/Prefabs/Effects/Enemy/EnemyHitEffect.prefab");
        enemyDeathEffect = LoadPrefab("Assets/Prefabs/Effects/Enemy/EnemyDeathEffect.prefab");
        projectileHitEffect = LoadPrefab("Assets/Prefabs/Effects/PlayerProjectileHit.prefab");
        victoryEffect = LoadPrefab("Assets/Prefabs/Effects/Win&Lose/VictoryEffect.prefab");
        gameOverEffect = LoadPrefab("Assets/Prefabs/Effects/Win&Lose/GameOverEffect.prefab");
#endif
    }

    private void CreateLevels()
    {
        levels = new[]
        {
            new LevelDefinition("Training Orbit", "Balanced first mission with light asteroids and slower enemies.", 500, 1f, 2.6f, 7.5f, 6, 1, 0),
            new LevelDefinition("Asteroid Belt", "More destructible rocks, faster enemy waves, and tighter movement space.", 700, 1.22f, 1.9f, 9.5f, 12, 2, 1),
            new LevelDefinition("Black Hole Raid", "Hard mode: aggressive enemies, fewer pickups, and a dangerous deep-space backdrop.", 900, 1.5f, 1.35f, 12.5f, 9, 3, 2)
        };
    }

    private void CreateBackground()
    {
        GameObject background = new GameObject("Level Background");
        background.transform.SetParent(worldRoot);
        background.transform.position = Vector3.forward;

        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        Sprite backgroundSprite = Pick(backgroundSprites, CurrentLevel.backgroundIndex);
        if (backgroundSprite != null)
        {
            renderer.sprite = backgroundSprite;
            renderer.sortingOrder = -20;
            background.transform.localScale = new Vector3(2.15f, 2.15f, 1f);
            return;
        }

        Texture2D texture = new Texture2D(256, 144);
        Color dark = new Color(0.015f, 0.018f, 0.05f, 1f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, dark);
            }
        }

        for (int i = 0; i < 180; i++)
        {
            int x = Random.Range(0, texture.width);
            int y = Random.Range(0, texture.height);
            float brightness = Random.Range(0.45f, 1f);
            texture.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();

        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
        renderer.sortingOrder = -20;
        background.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
    }

    private void CreateCanvas()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObject = new GameObject("Rubric Game UI");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
        canvasObject.AddComponent<GraphicRaycaster>();
        root = canvas.GetComponent<RectTransform>();

        hudPanel = CreatePanel("HUD", root, new Color(0, 0, 0, 0));
        Stretch(hudPanel);
        scoreText = CreateText("Score", hudPanel, "Score 0   High 0", 24, TextAnchor.UpperLeft);
        Anchor(scoreText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -20), new Vector2(560, 50));
        statusText = CreateText("Status", hudPanel, "", 22, TextAnchor.UpperRight);
        Anchor(statusText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-588, -20), new Vector2(560, 50));
        objectiveText = CreateText("Objective", hudPanel, "Objective: reach 500 points", 22, TextAnchor.LowerCenter);
        Anchor(objectiveText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-350, 18), new Vector2(700, 42));
        feedbackText = CreateText("Feedback", hudPanel, "", 30, TextAnchor.MiddleCenter);
        Anchor(feedbackText.rectTransform, new Vector2(0.5f, 0.77f), new Vector2(0.5f, 0.77f), new Vector2(-360, -35), new Vector2(720, 70));
        hudPanel.gameObject.SetActive(false);

        centerPanel = CreatePanel("Menu Panel", root, new Color(0.02f, 0.03f, 0.08f, 0.92f));
        Stretch(centerPanel);
    }

    private void ShowMenu()
    {
        state = GameState.Menu;
        ClearWorld();
        hudPanel.gameObject.SetActive(false);
        BuildMenu(
            "ASTEROID RESCUE",
            "Goal: survive the attack, destroy enemies and asteroids, collect power-ups, and clear one of three missions.",
            new MenuButton("Level Select", ShowLevelSelect),
            new MenuButton("Instructions", ShowInstructions),
            new MenuButton("Changes / Credits", ShowChanges),
            new MenuButton("Exit", QuitGame));
    }

    private void ShowLevelSelect()
    {
        state = GameState.Menu;
        ClearWorld();
        hudPanel.gameObject.SetActive(false);
        BuildMenu(
            "SELECT LEVEL",
            "Choose a mission. Each level uses different environment art, objectives, enemy pressure, asteroids, and pickup timing.",
            new MenuButton("1  Training Orbit", delegate { StartGame(0); }),
            new MenuButton("2  Asteroid Belt", delegate { StartGame(1); }),
            new MenuButton("3  Black Hole Raid", delegate { StartGame(2); }),
            new MenuButton("Back", ShowMenu));
    }

    private void ShowInstructions()
    {
        state = GameState.Instructions;
        BuildMenu(
            "INSTRUCTIONS",
            "Move with WASD or arrow keys. Aim with the mouse. Fire with left click or Space. Pick up green diamonds for repairs, shield time, or rapid fire. Press Esc during play to pause.",
            new MenuButton("Level Select", ShowLevelSelect),
            new MenuButton("Back", ShowMenu));
    }

    private void ShowChanges()
    {
        state = GameState.Changes;
        BuildMenu(
            "CHANGES AND CREDITS",
            "Added level select, three missions, live HUD, objective text, high score, destructible asteroids, scaling enemy waves, power-ups, pause/restart flow, hit flashes, captions, camera feedback, and project asset art/effects.\n\nCredits: game uses art, audio folders, and effect prefabs already included in this project. Runtime code and UI flow were created for this update.",
            new MenuButton("Back", ShowMenu));
    }

    private void StartGame()
    {
        StartGame(selectedLevelIndex);
    }

    private void StartGame(int levelIndex)
    {
        selectedLevelIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
        state = GameState.Playing;
        centerPanel.gameObject.SetActive(false);
        hudPanel.gameObject.SetActive(true);
        ClearWorld();

        score = 0;
        lives = 3;
        wave = 1;
        playTimer = 0;
        enemySpawnTimer = 0.8f;
        powerUpSpawnTimer = CurrentLevel.powerUpDelay;
        SpawnPlayer();
        SpawnAsteroidField();
        DecorateEnvironment();
        ShowFeedback(CurrentLevel.name + ": score " + CurrentLevel.targetScore + " points");
        UpdateHud();
    }

    private void PauseGame()
    {
        state = GameState.Paused;
        BuildMenu(
            "PAUSED",
            "Take a breath, then jump back in.",
            new MenuButton("Resume", ResumeGame),
            new MenuButton("Restart", StartGame),
            new MenuButton("Level Select", ShowLevelSelect),
            new MenuButton("Main Menu", ShowMenu));
    }

    private void ResumeGame()
    {
        state = GameState.Playing;
        centerPanel.gameObject.SetActive(false);
        hudPanel.gameObject.SetActive(true);
        ShowFeedback("Resumed");
    }

    private void WinGame()
    {
        state = GameState.Win;
        SaveHighScore();
        SpawnEffect(victoryEffect, Vector3.zero, 1f);
        BuildMenu(
            "MISSION COMPLETE",
            "You reached the rescue score. Final score: " + score + "\nTime: " + Mathf.FloorToInt(playTimer) + " seconds",
            new MenuButton("Play Again", StartGame),
            new MenuButton("Level Select", ShowLevelSelect),
            new MenuButton("Main Menu", ShowMenu));
    }

    private void GameOver()
    {
        state = GameState.GameOver;
        SaveHighScore();
        SpawnEffect(gameOverEffect, Vector3.zero, 1f);
        BuildMenu(
            "GAME OVER",
            "Final score: " + score + "\nHigh score: " + highScore,
            new MenuButton("Retry", StartGame),
            new MenuButton("Level Select", ShowLevelSelect),
            new MenuButton("Main Menu", ShowMenu));
    }

    private void UpdatePlaying()
    {
        playTimer += Time.deltaTime;
        wave = 1 + Mathf.FloorToInt(playTimer / 25f);
        enemySpawnTimer -= Time.deltaTime;
        powerUpSpawnTimer -= Time.deltaTime;

        if (enemySpawnTimer <= 0)
        {
            SpawnEnemy();
            enemySpawnTimer = Mathf.Max(0.5f, CurrentLevel.enemySpawnDelay - wave * 0.18f);
        }

        if (powerUpSpawnTimer <= 0)
        {
            SpawnPowerUp();
            powerUpSpawnTimer = Random.Range(CurrentLevel.powerUpDelay, CurrentLevel.powerUpDelay + 4f);
        }

        player.Tick(this);
        TickList(playerShots);
        TickList(enemyShots);
        TickList(enemies);
        TickList(powerUps);
        TickList(asteroids);
        CheckCollisions();
        CleanupDeadObjects();
        UpdateHud();

        if (score >= CurrentLevel.targetScore)
        {
            WinGame();
        }
    }

    private void SpawnPlayer()
    {
        GameObject go = CreateSpriteObject("Player", Pick(playerSprites, selectedLevelIndex) ?? playerSprite, new Vector3(0, -2.9f, 0), 0.62f, 5);
        player = go.AddComponent<PlayerShip>();
        player.Initialize(6.5f, 0.22f);
        go.transform.SetParent(worldRoot);
    }

    private void SpawnEnemy()
    {
        Vector2 side = Random.value > 0.5f ? new Vector2(Random.Range(-ArenaHalfWidth, ArenaHalfWidth), ArenaHalfHeight + 0.6f) : new Vector2(Random.value > 0.5f ? ArenaHalfWidth + 0.6f : -ArenaHalfWidth - 0.6f, Random.Range(-ArenaHalfHeight, ArenaHalfHeight));
        GameObject go = CreateSpriteObject("Enemy Wave " + wave, Pick(enemySprites, Random.Range(0, 3)) ?? enemySprite, side, 0.58f, 4);
        EnemyShip enemy = go.AddComponent<EnemyShip>();
        enemy.Initialize((1.35f + wave * 0.22f) * CurrentLevel.enemySpeedMultiplier, Mathf.Max(0.55f, 1.55f - wave * 0.08f), 2 + wave / 2);
        go.transform.SetParent(worldRoot);
        enemies.Add(enemy);
    }

    private void SpawnAsteroidField()
    {
        for (int i = 0; i < CurrentLevel.asteroidCount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-7.4f, 7.4f), Random.Range(-3.3f, 3.2f), 0);
            if (Vector3.Distance(position, Vector3.zero) < 2.2f)
            {
                position.x += 3.2f;
            }

            GameObject go = CreateSpriteObject("Destructible Asteroid", Pick(asteroidSprites, i) ?? asteroidSprite, position, Random.Range(0.55f, 1.05f), 2);
            AsteroidObstacle asteroid = go.AddComponent<AsteroidObstacle>();
            asteroid.Initialize(Random.Range(2, 5));
            go.transform.SetParent(worldRoot);
            asteroids.Add(asteroid);
        }
    }

    private void DecorateEnvironment()
    {
        Sprite station = Pick(stationSprites, selectedLevelIndex);
        if (station != null)
        {
            GameObject go = CreateSpriteObject("Level Landmark Space Station", station, new Vector3(-6.5f, 3.05f, 0), 1.25f, -5);
            go.transform.SetParent(worldRoot);
        }

        Sprite planet = Pick(planetSprites, selectedLevelIndex);
        if (planet != null)
        {
            GameObject go = CreateSpriteObject("Level Landmark Planet", planet, new Vector3(6.7f, -3.25f, 0), 1.65f, -6);
            go.transform.SetParent(worldRoot);
        }

        if (CurrentLevel.hazardTheme >= 3)
        {
            Sprite blackHole = Pick(blackHoleSprites, selectedLevelIndex);
            if (blackHole != null)
            {
                GameObject go = CreateSpriteObject("Black Hole Hazard Landmark", blackHole, new Vector3(5.5f, 3.1f, 0), 1.35f, -4);
                go.transform.SetParent(worldRoot);
            }
        }
    }

    private void SpawnPowerUp()
    {
        Vector3 position = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(-3.6f, 3.6f), 0);
        GameObject go = CreateSpriteObject("Power-Up", powerUpSprite, position, 0.42f, 3);
        PowerUpPickup pickup = go.AddComponent<PowerUpPickup>();
        pickup.Initialize((PowerUpType)Random.Range(0, 3));
        go.transform.SetParent(worldRoot);
        powerUps.Add(pickup);
        ShowFeedback("Power-up deployed");
    }

    public void FirePlayerShot(Vector3 position, Quaternion rotation)
    {
        GameObject go = CreateSpriteObject("Player Shot", shotSprite, position, 0.72f, 6);
        PlayerShot shot = go.AddComponent<PlayerShot>();
        shot.Initialize(rotation * Vector3.up, 11.5f, 2.2f);
        go.transform.SetParent(shotRoot);
        playerShots.Add(shot);
    }

    public void FireEnemyShot(Vector3 position, Vector3 direction)
    {
        GameObject go = CreateSpriteObject("Enemy Shot", enemyShotSprite, position, 0.62f, 6);
        go.GetComponent<SpriteRenderer>().color = Color.white;
        EnemyShot shot = go.AddComponent<EnemyShot>();
        shot.Initialize(direction, 5.8f + wave * 0.18f, 4f);
        go.transform.SetParent(shotRoot);
        enemyShots.Add(shot);
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < playerShots.Count; i++)
        {
            PlayerShot shot = playerShots[i];
            if (!shot.IsAlive)
            {
                continue;
            }

            for (int j = 0; j < enemies.Count; j++)
            {
                EnemyShip enemy = enemies[j];
                if (enemy.IsAlive && Touching(shot.transform, 0.16f, enemy.transform, 0.42f))
                {
                    SpawnEffect(projectileHitEffect, shot.transform.position, 0.6f);
                    shot.Kill();
                    enemy.TakeHit();
                    AddScore(35);
                    Shake(0.08f);
                    ShowFeedback("Enemy hit +35");
                    if (!enemy.IsAlive)
                    {
                        SpawnEffect(enemyDeathEffect, enemy.transform.position, 0.85f);
                    }
                    break;
                }
            }

            for (int j = 0; j < asteroids.Count; j++)
            {
                AsteroidObstacle asteroid = asteroids[j];
                if (asteroid.IsAlive && Touching(shot.transform, 0.16f, asteroid.transform, asteroid.Radius))
                {
                    SpawnEffect(projectileHitEffect, shot.transform.position, 0.6f);
                    shot.Kill();
                    asteroid.TakeHit();
                    AddScore(15);
                    Shake(0.05f);
                    ShowFeedback("Asteroid damaged +15");
                    break;
                }
            }
        }

        for (int i = 0; i < enemyShots.Count; i++)
        {
            EnemyShot shot = enemyShots[i];
            if (shot.IsAlive && player != null && player.IsAlive && Touching(shot.transform, 0.16f, player.transform, 0.42f))
            {
                SpawnEffect(playerHitEffect, player.transform.position, 0.7f);
                shot.Kill();
                DamagePlayer();
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyShip enemy = enemies[i];
            if (enemy.IsAlive && player != null && player.IsAlive && Touching(enemy.transform, 0.42f, player.transform, 0.45f))
            {
                enemy.Kill();
                AddScore(45);
                DamagePlayer();
            }
        }

        for (int i = 0; i < powerUps.Count; i++)
        {
            PowerUpPickup pickup = powerUps[i];
            if (pickup.IsAlive && player != null && player.IsAlive && Touching(pickup.transform, 0.36f, player.transform, 0.48f))
            {
                pickup.Kill();
                ApplyPowerUp(pickup.Type);
            }
        }
    }

    private void DamagePlayer()
    {
        if (player.AbsorbHit())
        {
            SpawnEffect(playerHitEffect, player.transform.position, 0.7f);
            ShowFeedback("Shield blocked the hit");
            return;
        }

        lives--;
        Shake(0.18f);
        player.Flash(Color.red);
        ShowFeedback("Hull hit! Lives: " + lives);
        if (lives <= 0)
        {
            SpawnEffect(playerDeathEffect, player.transform.position, 0.9f);
            player.Kill();
            GameOver();
        }
    }

    private void ApplyPowerUp(PowerUpType type)
    {
        if (type == PowerUpType.Repair)
        {
            lives = Mathf.Min(5, lives + 1);
            ShowFeedback("Repair collected: +1 life");
        }
        else if (type == PowerUpType.RapidFire)
        {
            player.SetRapidFire(7f);
            ShowFeedback("Rapid fire active");
        }
        else
        {
            player.SetShield(7f);
            ShowFeedback("Shield active");
        }

        AddScore(40);
    }

    private void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
        }
    }

    private void SaveHighScore()
    {
        if (score >= highScore)
        {
            PlayerPrefs.SetInt("RubricHighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    private void UpdateHud()
    {
        scoreText.text = "Score " + score + "   High " + highScore;
        statusText.text = "Lives " + lives + "   Wave " + wave + "   Time " + Mathf.FloorToInt(playTimer);
        objectiveText.text = CurrentLevel.name + " objective: reach " + CurrentLevel.targetScore + " points   Progress " + Mathf.Clamp(score, 0, CurrentLevel.targetScore) + "/" + CurrentLevel.targetScore;
    }

    private void ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.color = new Color(1f, 0.92f, 0.35f, 1f);
        feedbackTimer = 1.25f;
    }

    private void UpdateFeedback()
    {
        if (feedbackTimer <= 0)
        {
            feedbackText.text = "";
            return;
        }

        feedbackTimer -= Time.deltaTime;
        Color color = feedbackText.color;
        color.a = Mathf.Clamp01(feedbackTimer);
        feedbackText.color = color;
    }

    private void Shake(float amount)
    {
        shakeTimer = Mathf.Max(shakeTimer, amount);
    }

    private void UpdateCameraShake()
    {
        if (shakeTimer <= 0)
        {
            mainCamera.transform.position = cameraBasePosition;
            return;
        }

        shakeTimer -= Time.deltaTime;
        Vector2 offset = Random.insideUnitCircle * 0.08f;
        mainCamera.transform.position = cameraBasePosition + new Vector3(offset.x, offset.y, 0);
    }

    private void SpawnEffect(GameObject prefab, Vector3 position, float scale)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale *= scale;
    }

    private void CleanupDeadObjects()
    {
        Cleanup(playerShots);
        Cleanup(enemyShots);
        Cleanup(enemies);
        Cleanup(powerUps);
        Cleanup(asteroids);
    }

    private void ClearWorld()
    {
        for (int i = worldRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(worldRoot.GetChild(i).gameObject);
        }

        enemies.Clear();
        playerShots.Clear();
        enemyShots.Clear();
        asteroids.Clear();
        powerUps.Clear();
        player = null;
        shotRoot = new GameObject("Projectile Holder").transform;
        shotRoot.SetParent(worldRoot);
        CreateBackground();
    }

    private void BuildMenu(string title, string body, params MenuButton[] buttons)
    {
        hudPanel.gameObject.SetActive(state == GameState.Playing || state == GameState.Paused);
        centerPanel.gameObject.SetActive(true);

        for (int i = centerPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(centerPanel.GetChild(i).gameObject);
        }

        Text titleText = CreateText("Title", centerPanel, title, 48, TextAnchor.MiddleCenter);
        Anchor(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-430 + MenuContentOffsetX + MenuTitleExtraOffsetX, 120), new Vector2(860, 80));

        Text bodyText = CreateText("Body", centerPanel, body, 24, TextAnchor.MiddleCenter);
        Anchor(bodyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-455 + MenuContentOffsetX, -20), new Vector2(910, 150));

        float startY = -170;
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = CreateButton(buttons[i].Label, centerPanel, buttons[i].Action);
            Anchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-150 + MenuContentOffsetX, startY + MenuButtonOffsetY - i * 64), new Vector2(300, 48));
        }
    }

    private void QuitGame()
    {
        Application.Quit();
        ShowFeedback("Exit is available in a built game");
    }

    private GameObject CreateSpriteObject(string objectName, Sprite sprite, Vector3 position, float scale, int sortingOrder)
    {
        GameObject go = new GameObject(objectName);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private static bool Touching(Transform a, float radiusA, Transform b, float radiusB)
    {
        return Vector3.Distance(a.position, b.position) <= radiusA + radiusB;
    }

    private static Sprite Pick(Sprite[] sprites, int index)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return null;
        }

        int safeIndex = Mathf.Abs(index) % sprites.Length;
        return sprites[safeIndex];
    }

#if UNITY_EDITOR
    private static Sprite[] LoadSpritesAtPath(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> sprites = new List<Sprite>();
        foreach (Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        return sprites.ToArray();
    }

    private static Sprite[] LoadSpriteList(params string[] paths)
    {
        List<Sprite> sprites = new List<Sprite>();
        foreach (string path in paths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        return sprites.ToArray();
    }

    private static GameObject LoadPrefab(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
#endif

    private static void TickList<T>(List<T> items) where T : RuntimeActor
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Tick();
        }
    }

    private static void Cleanup<T>(List<T> items) where T : RuntimeActor
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (!items[i].IsAlive)
            {
                Destroy(items[i].gameObject);
                items.RemoveAt(i);
            }
        }
    }

    private static RectTransform CreatePanel(string objectName, Transform parent, Color color)
    {
        GameObject panel = new GameObject(objectName);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel.GetComponent<RectTransform>();
    }

    private static Text CreateText(string objectName, Transform parent, string value, int size, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + " Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = new Color(0.95f, 0.82f, 0.34f, 0.98f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.95f, 0.82f, 0.34f, 0.98f);
        colors.highlightedColor = new Color(1f, 0.94f, 0.55f, 1f);
        colors.pressedColor = new Color(0.72f, 0.48f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.7f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.onClick.AddListener(action);

        Text text = CreateText("Label", buttonObject.transform, label.ToUpperInvariant(), 20, TextAnchor.MiddleCenter);
        text.color = new Color(0.06f, 0.08f, 0.14f, 1f);
        text.fontStyle = FontStyle.Bold;
        Stretch(text.rectTransform);
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static Sprite MakeCircleSprite(Color fill, Color rim)
    {
        Texture2D texture = new Texture2D(48, 48);
        Vector2 center = new Vector2(23.5f, 23.5f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 22)
                {
                    texture.SetPixel(x, y, Color.Lerp(fill, rim, distance / 22f));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        return FinishSprite(texture);
    }

    private static Sprite MakeDiamondSprite(Color fill, Color rim)
    {
        Texture2D texture = new Texture2D(48, 48);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float d = Mathf.Abs(x - 24) + Mathf.Abs(y - 24);
                texture.SetPixel(x, y, d < 22 ? Color.Lerp(fill, rim, d / 22f) : Color.clear);
            }
        }
        return FinishSprite(texture);
    }

    private static Sprite MakeTriangleSprite(Color fill, Color accent)
    {
        Texture2D texture = new Texture2D(48, 48);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float halfWidth = Mathf.Lerp(4f, 21f, y / 47f);
                bool inside = y > 4 && Mathf.Abs(x - 24) < halfWidth;
                bool core = inside && Mathf.Abs(x - 24) < halfWidth * 0.32f;
                texture.SetPixel(x, y, inside ? (core ? accent : fill) : Color.clear);
            }
        }
        return FinishSprite(texture);
    }

    private static Sprite FinishSprite(Texture2D texture)
    {
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 48f);
    }

    private struct MenuButton
    {
        public readonly string Label;
        public readonly UnityEngine.Events.UnityAction Action;

        public MenuButton(string label, UnityEngine.Events.UnityAction action)
        {
            Label = label;
            Action = action;
        }
    }

    private struct LevelDefinition
    {
        public readonly string name;
        public readonly string description;
        public readonly int targetScore;
        public readonly float enemySpeedMultiplier;
        public readonly float enemySpawnDelay;
        public readonly float powerUpDelay;
        public readonly int asteroidCount;
        public readonly int hazardTheme;
        public readonly int backgroundIndex;

        public LevelDefinition(string name, string description, int targetScore, float enemySpeedMultiplier, float enemySpawnDelay, float powerUpDelay, int asteroidCount, int hazardTheme, int backgroundIndex)
        {
            this.name = name;
            this.description = description;
            this.targetScore = targetScore;
            this.enemySpeedMultiplier = enemySpeedMultiplier;
            this.enemySpawnDelay = enemySpawnDelay;
            this.powerUpDelay = powerUpDelay;
            this.asteroidCount = asteroidCount;
            this.hazardTheme = hazardTheme;
            this.backgroundIndex = backgroundIndex;
        }
    }

    public abstract class RuntimeActor : MonoBehaviour
    {
        public bool IsAlive { get; private set; } = true;
        protected float lifetime = Mathf.Infinity;
        private SpriteRenderer spriteRenderer;
        private float flashTimer;
        private Color baseColor = Color.white;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
        }

        public virtual void Tick()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
            {
                Kill();
            }

            if (flashTimer > 0 && spriteRenderer != null)
            {
                flashTimer -= Time.deltaTime;
                spriteRenderer.color = Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time * 16f, 1f));
                if (flashTimer <= 0)
                {
                    spriteRenderer.color = baseColor;
                }
            }
        }

        public void Flash(Color color)
        {
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
                spriteRenderer.color = color;
                flashTimer = 0.24f;
            }
        }

        public void Kill()
        {
            IsAlive = false;
        }
    }

    public class PlayerShip : RuntimeActor
    {
        private RubricGameBootstrap game;
        private float speed;
        private float normalFireDelay;
        private float fireDelay;
        private float fireTimer;
        private float shieldTimer;
        private float rapidTimer;

        public void Initialize(float moveSpeed, float shotDelay)
        {
            speed = moveSpeed;
            normalFireDelay = shotDelay;
            fireDelay = shotDelay;
        }

        public void Tick(RubricGameBootstrap owner)
        {
            game = owner;
            base.Tick();

            Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;
            transform.position += movement * speed * Time.deltaTime;
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -ArenaHalfWidth, ArenaHalfWidth), Mathf.Clamp(transform.position.y, -ArenaHalfHeight, ArenaHalfHeight), 0);

            Vector3 mouseWorld = game.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mouseWorld - transform.position;
            direction.z = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.up = direction.normalized;
            }

            fireTimer -= Time.deltaTime;
            shieldTimer -= Time.deltaTime;
            rapidTimer -= Time.deltaTime;
            fireDelay = rapidTimer > 0 ? 0.09f : normalFireDelay;

            if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && fireTimer <= 0)
            {
                game.FirePlayerShot(transform.position + transform.up * 0.48f, transform.rotation);
                fireTimer = fireDelay;
            }

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = shieldTimer > 0 ? new Color(0.3f, 1f, 0.7f) : Color.white;
            }
        }

        public bool AbsorbHit()
        {
            return shieldTimer > 0;
        }

        public void SetShield(float duration)
        {
            shieldTimer = duration;
        }

        public void SetRapidFire(float duration)
        {
            rapidTimer = duration;
        }
    }

    public class EnemyShip : RuntimeActor
    {
        private int health;
        private float speed;
        private float fireDelay;
        private float fireTimer;
        private RubricGameBootstrap game;

        public void Initialize(float moveSpeed, float shotDelay, int hitPoints)
        {
            speed = moveSpeed;
            fireDelay = shotDelay;
            health = hitPoints;
            game = FindObjectOfType<RubricGameBootstrap>();
        }

        public override void Tick()
        {
            base.Tick();
            if (!IsAlive || game == null || game.player == null)
            {
                return;
            }

            Vector3 direction = (game.player.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.up = direction;

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0)
            {
                game.FireEnemyShot(transform.position + direction * 0.42f, direction);
                fireTimer = fireDelay;
            }
        }

        public void TakeHit()
        {
            health--;
            Flash(Color.white);
            if (health <= 0)
            {
                Kill();
            }
        }
    }

    public abstract class MovingShot : RuntimeActor
    {
        private Vector3 velocity;

        public void Initialize(Vector3 direction, float speed, float life)
        {
            velocity = direction.normalized * speed;
            lifetime = life;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.up = direction.normalized;
            }
        }

        public override void Tick()
        {
            base.Tick();
            transform.position += velocity * Time.deltaTime;
            if (Mathf.Abs(transform.position.x) > ArenaHalfWidth + 1.5f || Mathf.Abs(transform.position.y) > ArenaHalfHeight + 1.5f)
            {
                Kill();
            }
        }
    }

    public class PlayerShot : MovingShot
    {
    }

    public class EnemyShot : MovingShot
    {
    }

    public class AsteroidObstacle : RuntimeActor
    {
        private int health;
        public float Radius { get; private set; }

        public void Initialize(int hitPoints)
        {
            health = hitPoints;
            Radius = transform.localScale.x * 0.42f;
        }

        public void TakeHit()
        {
            health--;
            Flash(Color.white);
            transform.localScale *= 0.92f;
            Radius = transform.localScale.x * 0.42f;
            if (health <= 0)
            {
                Kill();
            }
        }
    }

    public enum PowerUpType
    {
        Repair,
        RapidFire,
        Shield
    }

    public class PowerUpPickup : RuntimeActor
    {
        public PowerUpType Type { get; private set; }

        public void Initialize(PowerUpType type)
        {
            Type = type;
            lifetime = 11f;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = type == PowerUpType.Repair ? new Color(0.35f, 1f, 0.45f) : type == PowerUpType.RapidFire ? new Color(1f, 0.84f, 0.18f) : new Color(0.25f, 0.72f, 1f);
            }
        }

        public override void Tick()
        {
            base.Tick();
            transform.Rotate(0, 0, 120f * Time.deltaTime);
            float scale = 0.42f + Mathf.Sin(Time.time * 5f) * 0.04f;
            transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
