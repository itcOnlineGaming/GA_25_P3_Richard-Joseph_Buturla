# Unity Tower Defense - Tower System Package

## Overview

This package provides a core framework for implementing towers within a tower defense game in Unity. It handles tower data management, basic tower lifecycle (spawning, upgrading, repairing, destroying), target acquisition, and includes an event system for decoupling game logic. An example tower implementation and a performance testing suite are also included.

## Features

* **Abstract `Tower` Base Class:** Easily create new, custom tower types by inheriting from `Tower`.
* **Data-Driven Stats:** Define tower statistics (cost, damage, health, range, etc.) per level using `TowerData`.
* **Centralized Data Management:** `TowerFactory` manages the creation and retrieval of `TowerData`.
* **Target Management:** `TowerDefenseManager` (Singleton) keeps track of all active targets (`ITargetable`) for towers to query.
* **Event System:** `TowerDefenseEvents` provides static C# events for key tower actions (Placed, Upgraded, Destroyed, Repaired), allowing other systems to react.
* **Utility Functions:** `TowerUtils` offers static methods for common actions like spawning, upgrading, and repairing towers.
* **Example Implementation:** Includes `CannonBallTower`, `CannonBallShootBehaviour`, and `CanonBallProjectile` showing how to create a functional tower with attack logic.
* **Performance Testing Suite:** Includes `TowerPerformanceTestManager` and `TestDummy` to automate testing of tower performance across levels and generate CSV reports.

## Core Components

* **`Tower` (Abstract Class):**
    * The base script for all tower GameObjects.
    * Handles health, finding the nearest target (`FindNearestTarget`), basic initialization.
    * Requires a `TowerData` object for its stats.
    * Must be inherited by your specific tower scripts (e.g., `CannonBallTower`, `LaserTower`).
* **`TowerData` (Class):**
    * Holds all statistics for a specific tower type at a specific level (Cost, MaxHealth, Damage, FireRate, Range, Icon, etc.).
    * Contains the `TowerLevel` enum (LevelOne, LevelTwo, LevelThree).
    * Contains the `TowerID` struct for unique identification.
* **`TowerFactory` (Static Class):**
    * Acts as a central registry for all `TowerData`.
    * **Crucial:** You must populate this factory with data for *all* your tower types and levels using `TowerFactory.CreateTowerData()` during game initialization.
    * Use `TowerFactory.GetTowerData()` to retrieve the stats needed to initialize or upgrade a tower.
* **`TowerUtils` (Static Class):**
    * Provides helper methods:
        * `SpawnTower()`: Instantiates a tower prefab and initializes it.
        * `UpgradeBuilding()`: Upgrades a tower to the next level using data from `TowerFactory`.
        * `RepairBuilding()`: Repairs a tower.
* **`TowerDefenseManager` (Singleton):**
    * Manages a list of all active `ITargetable` entities (enemies).
    * Towers use `TowerDefenseManager.Instance.GetAllTargets()` to find potential targets.
    * Your enemy units *must* register/unregister themselves using `RegisterTarget()` and `UnregisterTarget()`.
* **`TowerDefenseEvents` (Static Class):**
    * Provides static C# events: `OnTowerPlaced`, `OnTowerUpgraded`, `OnTowerDestroyed`, `OnTowerRepaired`.
    * Subscribe to these events from other systems (UI, Audio, Game Managers) to react to tower state changes.
* **`ITargetable` (Interface):**
    * Interface that enemies (or other targetable objects) must implement.
    * Defines properties like `Transform`, `IsAlive`, `Position` and the `TakeDamage()` method.

## Getting Started

1.  **Import Package:** Add the package to your Unity project.
2.  **Manager Setup:** Ensure a `TowerDefenseManager` component exists in your main game scene. You can add the script to an empty GameObject; it will create itself if not found (Singleton pattern).
3.  **Define Tower Data:**
    * **This is critical.** This system defines tower stats *in code*.
    * Find a suitable place that runs early in your game's initialization (e.g., a `GameManager` script's `Awake()` or `Start()` method).
    * For **each level** of **each tower type** you have, call `TowerFactory.CreateTowerData()`.
    ```csharp
    // Example in a GameManager Awake()
    void Awake()
    {
        // Sniper Tower - Level 1
        TowerFactory.CreateTowerData(
            towerType: "Sniper",
            level: TowerData.TowerLevel.LevelOne,
            maxHealth: 700,
            buyCost: 550,
            upgradeCost: 650, // Cost to upgrade TO level 2
            rebuildCost: 550, // Or calculated value
            fireRate: 0.5f, // Time between shots
            damage: 60f,
            range: 15f,
            icon: null // Assign your UI icon sprite here
        );

        // Sniper Tower - Level 2
        TowerFactory.CreateTowerData(
            towerType: "Sniper",
            level: TowerData.TowerLevel.LevelTwo,
            maxHealth: 1050,
            buyCost: 0, // Buy cost usually 0 for upgrades
            upgradeCost: 1150, // Cost to upgrade TO level 3
            rebuildCost: 1200, // Or calculated value
            fireRate: 0.4f,
            damage: 120f,
            range: 18f,
            icon: null
        );

         // Sniper Tower - Level 3
        TowerFactory.CreateTowerData(
            towerType: "Sniper",
            level: TowerData.TowerLevel.LevelThree,
            maxHealth: 1575,
            buyCost: 0,
            upgradeCost: 0, // No further upgrade
            rebuildCost: 1800,
            fireRate: 0.3f,
            damage: 250f,
            range: 20f,
            icon: null
        );

        // --- Define data for other towers (e.g., "Cannon", "Laser") similarly ---
    }
    ```
4.  **Implement Enemies (`ITargetable`):**
    * Create your enemy scripts.
    * Make them implement the `ITargetable` interface.
    * In their `OnEnable()` or `Start()`, call `TowerDefenseManager.Instance.RegisterTarget(this);`.
    * In their `OnDisable()` or `OnDestroy()`, call `TowerDefenseManager.Instance.UnregisterTarget(this);`.
    * Implement the `TakeDamage()` method logic.
5.  **Spawning Towers:**
    * Get the desired `TowerData` using `TowerFactory.GetTowerData(towerTypeName, TowerData.TowerLevel.LevelOne);`.
    * Get the corresponding tower Prefab (you need to create these).
    * Call `TowerUtils.SpawnTower(position, rotation, towerData, towerPrefab);`.
6.  **Upgrading/Repairing:**
    * Use `TowerUtils.UpgradeBuilding(towerInstance);`
    * Use `TowerUtils.RepairBuilding(towerInstance, amountToRepair);`
7.  **Using Events:**
    * Subscribe to events in `TowerDefenseEvents` from your UI, audio, or game logic managers.
    ```csharp
    void OnEnable()
    {
        TowerDefenseEvents.OnTowerPlaced += HandleTowerPlaced;
        TowerDefenseEvents.OnTowerDestroyed += HandleTowerDestroyed;
    }

    void OnDisable()
    {
        TowerDefenseEvents.OnTowerPlaced -= HandleTowerPlaced;
        TowerDefenseEvents.OnTowerDestroyed -= HandleTowerDestroyed;
    }

    void HandleTowerPlaced(Tower tower)
    {
        Debug.Log($"{tower.GetTowerData().TowerType} placed!");
        // Update UI, play sound, etc.
    }

    void HandleTowerDestroyed(Tower tower)
    {
         Debug.Log($"{tower.GetTowerData().TowerType} destroyed!");
        // Update UI, trigger game logic, etc.
    }
    ```

## Creating a New Tower Type (Example: Laser Tower)

1.  **Create Script:** Create a new C# script, e.g., `LaserTower`, and make it inherit from `Tower`.
    ```csharp
    using UnityEngine;

    public class LaserTower : Tower
    {
        public LineRenderer laserBeam; // Assign in Inspector
        public float beamDuration = 0.1f; // How long beam stays visible
        private float lastFireTime = -Mathf.Infinity;
        private ITargetable currentTarget;
        private float beamTimer = 0f;

        public override void Initialise(TowerData data)
        {
            base.Initialise(data);
            laserBeam.enabled = false;
            // Any other laser-specific setup
        }

        public override void Update()
        {
            if (!placed) return;

            // Hide beam after duration
            if(laserBeam.enabled && Time.time > beamTimer + beamDuration)
            {
                laserBeam.enabled = false;
            }

            // Target acquisition
            if (currentTarget == null || !currentTarget.IsAlive)
            {
                currentTarget = FindNearestTarget(); // Uses base Tower method
                if (currentTarget == null)
                {
                    laserBeam.enabled = false; // No target, hide beam
                    return; // Exit if no target found
                }
            }

            // Check range again in case target moved out
            if (Vector3.Distance(transform.position, currentTarget.Position) > range)
            {
                 currentTarget = null; // Lose target
                 laserBeam.enabled = false;
                 return;
            }

            // Aiming (Optional: Rotate tower/turret towards currentTarget.Position)
            // ... Your aiming logic here ...

            // Firing logic
            if (Time.time >= lastFireTime + towerData.FireRate)
            {
                lastFireTime = Time.time;
                FireLaser();
            }
        }

        void FireLaser()
        {
            if (currentTarget == null || !currentTarget.IsAlive) return;

            // Apply damage
            currentTarget.TakeDamage(towerData.Damage);

            // Visuals
            laserBeam.enabled = true;
            laserBeam.SetPosition(0, firePoint.position); // firePoint is Transform field from base Tower
            laserBeam.SetPosition(1, currentTarget.Position);
            beamTimer = Time.time;

            // Optional: Play sound, particle effects etc.
        }
    }
    ```
2.  **Create Prefab:**
    * Create a new empty GameObject or use your tower model.
    * Add the `LaserTower` script to it.
    * Add a `LineRenderer` component for the laser visual.
    * Assign the `LineRenderer` component to the `Laser Beam` field in the `LaserTower` script inspector.
    * Assign a child GameObject to the `Fire Point` field (inherited from `Tower`) to specify where the laser originates.
    * Save this GameObject as a Prefab (e.g., `LaserTowerPrefab`).
3.  **Define Data:** In your game initialization code (where you called `TowerFactory.CreateTowerData` before), add entries for all levels of your "Laser" tower.
    ```csharp
     // Example Laser Tower Data (in GameManager Awake())
     TowerFactory.CreateTowerData("Laser", TowerData.TowerLevel.LevelOne, ... );
     TowerFactory.CreateTowerData("Laser", TowerData.TowerLevel.LevelTwo, ... );
     TowerFactory.CreateTowerData("Laser", TowerData.TowerLevel.LevelThree, ... );
    ```
4.  **Spawn:** When spawning, retrieve the "Laser" `TowerData` and use the `LaserTowerPrefab`.
    ```csharp
    TowerData laserData = TowerFactory.GetTowerData("Laser", TowerData.TowerLevel.LevelOne);
    GameObject laserPrefab = Resources.Load<GameObject>("Prefabs/LaserTowerPrefab"); // Or your prefab loading method
    TowerUtils.SpawnTower(spawnPosition, Quaternion.identity, laserData, laserPrefab);
    ```

## Performance Testing

This package includes an optional testing suite:
* **`TowerPerformanceTestManager`:** Place this script on a GameObject in a test scene. Configure it in the inspector to select the tower prefab name (`towerName`), test duration, dummy types, etc.
![imageOne](https://github.com/user-attachments/assets/a3c0ea6f-2bdc-4013-85d0-2e54eed2a453)
* **`TestDummy`:** The target used by the test manager. Prefab can be assigned in the manager. Implements `ITargetable`.
* **Operation:** When entering Play mode in the test scene, the manager will:
    1.  Spawn the specified tower.
    2.  Spawn the configured test dummies.
    3.  Run the test for the specified duration, collecting metrics (`TowerPerformanceMetrics`).
    4.  Upgrade the tower and repeat for subsequent levels (`maxLevelsToTest`).
    5.  Calculate a rating (`TowerRatingCalculator`, `TowerRatingWeights`).
    6.  Log results to the console.
    7.  Optionally export detailed metrics to a CSV file in the `Assets/[csvOutputPath]` folder.
    8.  Optionally exit Play mode automatically.

This is useful for balancing towers and analyzing their performance under controlled conditions.
