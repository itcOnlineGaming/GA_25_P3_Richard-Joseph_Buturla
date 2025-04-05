using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

#region Core Testing System

/// <summary>
/// Main manager class for tower performance testing
/// </summary>
public class TowerPerformanceTestManager : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private string towerName = "DefaultTower";
    [SerializeField] private float testDurationPerLevel = 30f;
    [SerializeField] private int maxLevelsToTest = 3;
    [SerializeField] private bool autoExitPlayModeOnComplete = true;
    [SerializeField] private bool autoCaptureCSV = true;
    [SerializeField] private string csvOutputPath = "TowerTests";

    [Header("Test Target Setup")]
    [SerializeField] private TestDummyType dummyType = TestDummyType.Single;
    [SerializeField] private int numberOfDummies = 1;
    [SerializeField] private GameObject testDummyPrefab;
    [SerializeField] private Transform testArea;

    [Header("Rating Weights")]
    [SerializeField] private TowerRatingWeights ratingWeights;

    // Test state tracking
    private float testStartTime;
    private float levelStartTime;
    private int currentLevel = 1;
    private Tower testedTower;
    private List<TestDummy> activeTestTargets = new List<TestDummy>();
    private bool testFinished = false;
    private TowerPerformanceMetrics currentMetrics;
    private List<TowerPerformanceMetrics> allMetrics = new List<TowerPerformanceMetrics>();

    public enum TestDummyType
    {
        Single,
        Group,
        Moving
    }

    private void Start()
    {

        CreatedTowers.Instance.InitialiseTowers();

        // Initialize rating weights if not assigned
        if (ratingWeights == null)
        {
            ratingWeights = new TowerRatingWeights();
        }

        // Create the initial test setup
        testStartTime = Time.time;
        levelStartTime = testStartTime;
        // Setup metrics for first level
        currentMetrics = new TowerPerformanceMetrics(towerName, 1);

        // Spawn the tower
        SpawnTowerForTesting();

        // Spawn test dummies
        SpawnTestDummies();

        Debug.Log($"[Tower Test] Beginning performance test for tower: {towerName}");
    }

    private void Update()
    {
        if (testFinished)
            return;

        float currentDuration = Time.time - levelStartTime;

        // Track metrics during the test
        UpdateMetrics();

        // Check if it's time to proceed to the next level
        if (currentDuration >= testDurationPerLevel)
        {
            CompleteLevelTest();

            if (currentLevel >= maxLevelsToTest)
            {
                CompleteFullTest();
            }
            else
            {
                SetupNextLevelTest();
            }
        }
    }

    private void SpawnTowerForTesting()
    {
        string towerPrefabPath = "Prefabs/" + towerName;
        GameObject towerPrefab = Resources.Load<GameObject>(towerPrefabPath);

        if (towerPrefab == null)
        {
            Debug.LogError($"[Tower Test] Failed to load tower prefab at path: {towerPrefabPath}");
            testFinished = true;
            return;
        }

        Vector3 towerPosition = transform.position;
        Quaternion rotation = Quaternion.identity;

        testedTower = TowerUtils.SpawnTower(
            towerPosition,
            rotation,
            TowerFactory.GetTowerData(towerName, TowerData.TowerLevel.LevelOne),
            towerPrefab
        );

        if (testedTower == null)
        {
            Debug.LogError("[Tower Test] Failed to spawn tower for testing");
            testFinished = true;
            return;
        }

        currentMetrics.Cost = GetTowerCostForLevel(currentLevel);
    }

    private void SpawnTestDummies()
    {
        // Clear any existing test dummies
        foreach (var dummy in activeTestTargets)
        {
            if (dummy != null)
            {
                Destroy(dummy.gameObject);
            }
        }
        activeTestTargets.Clear();

        Vector3 basePosition = new Vector3(0f, 0f, 10f);

        switch (dummyType)
        {
            case TestDummyType.Single:
                SpawnSingleDummy(basePosition);
                break;

            case TestDummyType.Group:
                SpawnGroupDummies(basePosition);
                break;

            case TestDummyType.Moving:
                SpawnMovingDummies(basePosition);
                break;
        }

        // Register all dummies with the tower defense manager
        foreach (var dummy in activeTestTargets)
        {
            TowerDefenseManager.Instance.RegisterTarget(dummy);
        }
    }

    private void SpawnSingleDummy(Vector3 position)
    {
        TestDummy dummy = SpawnDummy(position);
        dummy.OnDamaged += OnDummyDamaged;
        dummy.OnDeath += OnDummyKilled;
        activeTestTargets.Add(dummy);
    }

    private void SpawnGroupDummies(Vector3 basePosition)
    {
        float spacing = 2f;

        for (int i = 0; i < numberOfDummies; i++)
        {
            // Create a grid or circle formation
            float angle = (360f / numberOfDummies) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * spacing, 0, Mathf.Sin(angle) * spacing);

            TestDummy dummy = SpawnDummy(basePosition + offset);
            dummy.OnDamaged += OnDummyDamaged;
            dummy.OnDeath += OnDummyKilled;
            activeTestTargets.Add(dummy);
        }
    }

    private void SpawnMovingDummies(Vector3 basePosition)
    {
        TestDummy dummy = SpawnDummy(basePosition);
        dummy.OnDamaged += OnDummyDamaged;
        dummy.OnDeath += OnDummyKilled;

        MovingTestDummy movingDummy = dummy.gameObject.AddComponent<MovingTestDummy>();
        movingDummy.Setup(basePosition, 5f, 2f); // Set movement radius and speed

        activeTestTargets.Add(dummy);
    }

    private TestDummy SpawnDummy(Vector3 position)
    {
        GameObject dummyObject;

        if (testDummyPrefab != null)
        {
            dummyObject = Instantiate(testDummyPrefab, position, Quaternion.identity);
        }
        else
        {
            dummyObject = new GameObject("TestDummy");
            dummyObject.transform.position = position;

            if (!dummyObject.GetComponent<MeshFilter>())
            {
                dummyObject.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                dummyObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }
        }

        TestDummy testDummy = dummyObject.GetComponent<TestDummy>();
        if (testDummy == null)
        {
            testDummy = dummyObject.AddComponent<TestDummy>();
            testDummy.Setup(100f, true); // Default health and auto-respawn
        }

        return testDummy;
    }

    private void UpdateMetrics()
    {
        currentMetrics.TestDuration = Time.time - levelStartTime;

        // Most metrics are updated via events (damage, kills, etc.)
        currentMetrics.CalculateDPS();

        // Update any tower-specific stats
        if (testedTower != null)
        {
            // Update survivability metrics if tower can be damaged
            currentMetrics.Health = testedTower.GetCurrentHealth();
            currentMetrics.MaxHealth = testedTower.GetTowerData().MaxHealth;

            // If tower has attack speed tracking, update attack consistency metrics
            currentMetrics.UpdateAttackSpeedStats(testedTower.GetTowerData().FireRate);
        }
    }

    private void OnDummyDamaged(float damage, bool isHit, int targetsHit, float overkillAmount)
    {
        currentMetrics.TotalDamage += damage;

        if (overkillAmount > 0)
        {
            currentMetrics.OverkillDamage += overkillAmount;
        }

        if (isHit)
        {
            currentMetrics.HitCount++;
        }
        else
        {
            currentMetrics.MissCount++;
        }

        if (targetsHit > 0)
        {
            currentMetrics.AddAOEHit(targetsHit);
        }

        // Track peak DPS by checking if the current raw DPS is higher than recorded
        float instantDPS = damage / Time.deltaTime;
        if (instantDPS > currentMetrics.PeakDPS)
        {
            currentMetrics.PeakDPS = instantDPS;
        }
    }

    private void OnDummyKilled()
    {
        currentMetrics.Kills++;
    }

    private void CompleteLevelTest()
    {
        // Finalize metrics calculations
        currentMetrics.CalculateFinalMetrics();

        // Calculate rating
        TowerRatingCalculator ratingCalculator = new TowerRatingCalculator(ratingWeights);
        float rating = ratingCalculator.CalculateRating(currentMetrics);
        currentMetrics.TowerRating = rating;

        // Log level results
        LogLevelResults();

        // Add to collection
        allMetrics.Add(currentMetrics);
    }

    private void SetupNextLevelTest()
    {
        currentLevel++;
        levelStartTime = Time.time;

        // Upgrade the tower
        TowerUtils.UpgradeBuilding(testedTower);

        // Reset test dummies
        foreach (var dummy in activeTestTargets)
        {
            if (dummy != null)
            {
                dummy.Reset();
            }
        }

        // Create new metrics object for this level
        currentMetrics = new TowerPerformanceMetrics(towerName, currentLevel);
        currentMetrics.Cost = GetTowerCostForLevel(currentLevel);

        Debug.Log($"[Tower Test] Beginning test for {towerName} level {currentLevel}");
    }

    private void CompleteFullTest()
    {
        testFinished = true;

        Debug.Log("[Tower Test] ====== Full Test Complete ======");
        LogFullResults();

        if (autoCaptureCSV)
        {
            ExportResultsToCSV();
        }

        if (autoExitPlayModeOnComplete && Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
        }
    }

    private void LogLevelResults()
    {
        Debug.Log($"[Tower Test] ==== Level {currentLevel} Results ====");
        Debug.Log($"Tower: {towerName} | Level: {currentLevel} | Rating: {currentMetrics.TowerRating:F1}/100");
        Debug.Log($"DPS (Avg): {currentMetrics.AverageDPS:F1} | Effective DPS: {currentMetrics.EffectiveDPS:F1} | Total Damage: {currentMetrics.TotalDamage:F0}"); // Added Effective DPS
        Debug.Log($"Kills: {currentMetrics.Kills} | Accuracy: {currentMetrics.Accuracy * 100:F1}%");
        Debug.Log($"Overkill: {currentMetrics.OverkillDamage:F0} ({currentMetrics.OverkillPercentage * 100:F1}%) | AOE Hits Avg: {currentMetrics.AverageAOEHits:F1}"); // Added Overkill %
    }

    private void LogFullResults()
    {
        Debug.Log("[Tower Test] ====== Summary ======");

        for (int i = 0; i < allMetrics.Count; i++)
        {
            var metrics = allMetrics[i];
            Debug.Log($"Level {i + 1}: DPS: {metrics.AverageDPS:F1} | Rating: {metrics.TowerRating:F1}/100 | Eff.DPS: {metrics.EffectiveDPS:F1} | Overkill: {metrics.OverkillPercentage * 100:F1}%"); // Added extra info
        }

        // Find best value level (based on overall rating vs cost)
        int bestValueIndex = 0;
        float bestValueRatio = 0;

        for (int i = 0; i < allMetrics.Count; i++)
        {
            if (allMetrics[i].Cost > 0)
            {
                float valueRatio = allMetrics[i].TowerRating / allMetrics[i].Cost;
                if (valueRatio > bestValueRatio)
                {
                    bestValueRatio = valueRatio;
                    bestValueIndex = i;
                }
            }
        }
        if (allMetrics.Count > 0 && allMetrics[bestValueIndex].Cost > 0)
        {
            Debug.Log($"Best Value Level (Rating/Cost): {bestValueIndex + 1} (Ratio: {bestValueRatio:F3})");
        }
        else
        {
            Debug.Log("Best Value Level (Rating/Cost): N/A (No cost data or no levels tested)");
        }
    }

    private float GetTowerCostForLevel(int level)
    {
        TowerData.TowerLevel towerLevel = (TowerData.TowerLevel)(level-1); // zero indexing so minus 1
        var towerData = TowerFactory.GetTowerData(towerName, towerLevel);

        if (towerData != null)
        {
            return towerData.Cost;
        }

        return 0f;
    }

    private void ExportResultsToCSV()
    {
        if (allMetrics.Count == 0)
            return;

        CSVWriter csvWriter = new CSVWriter();
        string fileName = $"{towerName}_TestResults_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        string directory = Path.Combine(Application.dataPath, csvOutputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, fileName);
        csvWriter.WriteMetricsToCSV(allMetrics, filePath);

        Debug.Log($"[Tower Test] CSV data exported to: {filePath}");
    }
}

/// <summary>
/// Holds all performance metrics for a tower at a specific level
/// </summary>
public class TowerPerformanceMetrics
{
    // Tower Identity
    public string TowerName { get; private set; }
    public int Level { get; private set; }
    public float Cost { get; set; } 

    // Time
    public float TestDuration { get; set; }

    // Damage Metrics
    public float TotalDamage { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public float OverkillDamage { get; set; }
    public int Kills { get; set; }
    public float AverageDPS { get; private set; }
    public float PeakDPS { get; set; }
    public float EffectiveDPS { get; private set; } // DPS excluding overkill
    public float OverkillPercentage { get; private set; } // Percentage of damage wasted

    // AOE Metrics
    private List<int> aoeHits = new List<int>();
    public float AverageAOEHits { get; private set; }

    // Attack Speed Metrics
    private List<float> attackIntervals = new List<float>();
    public float MinAttackInterval { get; private set; }
    public float MaxAttackInterval { get; private set; }
    public float AvgAttackInterval { get; private set; }
    public float AttackConsistency { get; private set; } // 0-1 value, higher is more consistent

    // Survivability
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float TimeAlive { get; set; } = float.PositiveInfinity; // For towers that can die

    // Effect Metrics
    public int EffectProcs { get; set; }
    public float EffectProcRate { get; private set; }

    // Final Score
    public float TowerRating { get; set; }

    // Calculated Properties
    public float Accuracy => HitCount > 0 ? (float)HitCount / (HitCount + MissCount) : 0f;

    public TowerPerformanceMetrics(string towerName, int level)
    {
        TowerName = towerName;
        Level = level;
        ResetMetrics();
    }

    public void ResetMetrics()
    {
        TotalDamage = 0;
        HitCount = 0;
        MissCount = 0;
        OverkillDamage = 0;
        Kills = 0;
        AverageDPS = 0;
        PeakDPS = 0;
        TestDuration = 0;
        EffectiveDPS = 0;         
        OverkillPercentage = 0;   

        aoeHits.Clear();
        attackIntervals.Clear();

        MinAttackInterval = float.MaxValue;
        MaxAttackInterval = 0;
        AvgAttackInterval = 0;
        AttackConsistency = 1;

        EffectProcs = 0;
        EffectProcRate = 0;
    }

    public void CalculateDPS()
    {
        if (TestDuration > 0)
        {
            AverageDPS = TotalDamage / TestDuration;
            // Calculate Effective DPS here as well, as TestDuration is known
            EffectiveDPS = (TotalDamage - OverkillDamage) / TestDuration;
        }
        else
        {
            AverageDPS = 0;
            EffectiveDPS = 0;
        }
    }

    public void AddAOEHit(int targetsHit)
    {
        aoeHits.Add(targetsHit);
    }

    public void UpdateAttackSpeedStats(float attackInterval)
    {
        if (attackInterval <= 0)
            return;

        attackIntervals.Add(attackInterval);

        // Update min/max
        if (attackInterval < MinAttackInterval)
        {
            MinAttackInterval = attackInterval;
        }
        if (attackInterval > MaxAttackInterval)
        {
            MaxAttackInterval = attackInterval;
        }
    }

    public void CalculateFinalMetrics()
    {
        // Calculate final DPS (Average and Effective)
        CalculateDPS();

        // Calculate AOE average
        if (aoeHits.Count > 0)
        {
            AverageAOEHits = (float)aoeHits.Average();
        }
        else
        {
            AverageAOEHits = 0; // Avoid NaN if no hits
        }


        // Calculate attack interval stats
        if (attackIntervals.Count > 0)
        {
            AvgAttackInterval = attackIntervals.Average();

            // Calculate consistency as 1 - (standard deviation / average)
            if (AvgAttackInterval > 0)
            {
                // Standard Deviation calculation
                float sumOfSquares = attackIntervals.Sum(i => Mathf.Pow(i - AvgAttackInterval, 2));
                float variance = sumOfSquares / attackIntervals.Count;
                float stdDev = Mathf.Sqrt(variance);
                AttackConsistency = Mathf.Clamp01(1f - (stdDev / AvgAttackInterval));
            }
            else
            {
                AttackConsistency = 1; // Perfectly consistent if average is 0? Or maybe 0? Let's stick to 1 if only one hit perhaps.
            }
        }
        else
        {
            // Handle cases with zero or one attack interval recorded
            MinAttackInterval = 0;
            MaxAttackInterval = 0;
            AvgAttackInterval = 0;
            AttackConsistency = 1; // Or 0, depending on desired interpretation
        }

        // Calculate Overkill Percentage
        if (TotalDamage > 0)
        {
            OverkillPercentage = OverkillDamage / TotalDamage;
        }
        else
        {
            OverkillPercentage = 0; // Avoid division by zero
        }

        // Calculate effect proc rate
        if (HitCount > 0)
        {
            EffectProcRate = (float)EffectProcs / HitCount;
        }
        else
        {
            EffectProcRate = 0; // Avoid division by zero
        }
    }
}

/// <summary>
/// Calculates a tower rating score based on weighted factors
/// </summary>
public class TowerRatingCalculator
{
    private TowerRatingWeights weights;

    public TowerRatingCalculator(TowerRatingWeights ratingWeights)
    {
        weights = ratingWeights ?? new TowerRatingWeights();
    }

    public float CalculateRating(TowerPerformanceMetrics metrics)
    {
        // Combat score (up to 100 points for each subcategory)
        float dpsScore = NormalizeValueAgainstBaseline(metrics.AverageDPS, 50f, 100f); 
        float accuracyScore = metrics.Accuracy * 100f;
        float peakDpsScore = NormalizeValueAgainstBaseline(metrics.PeakDPS, 80f, 150f);
        float aoeScore = NormalizeValueAgainstBaseline(metrics.AverageAOEHits, 1f, 5f); 

        // Weighted Combat Score
        float combatScore = (dpsScore * 0.5f) + (accuracyScore * 0.2f) + (peakDpsScore * 0.2f) + (aoeScore * 0.1f);

        // Survivability score
        float survivalScore = 100f; // Default for towers that don't take damage
        if (metrics.MaxHealth > 0)
        {
            float healthScore = NormalizeValueAgainstBaseline(metrics.MaxHealth, 50f, 200f); // Baseline/Top values might need tuning
            // Potentially factor in TimeAlive if towers can die
            survivalScore = healthScore;
        }

        // Overkill Penalty Score: Higher score for LOWER overkill percentage.
        // Maps 0% overkill to 100 score, 50% overkill to 50 score, 100%+ overkill to 0 score.
        // Using OverkillPercentage directly (0.0 to 1.0)
        float overkillPenaltyScore = Mathf.Clamp01(1.0f - (metrics.OverkillPercentage / 0.5f)) * 100f; // Simple linear mapping, 50% overkill = 0 score.

        float efficiencyScore = overkillPenaltyScore; 

        // Utility score
        float consistencyScore = metrics.AttackConsistency * 100f;
        float effectScore = metrics.EffectProcRate * 100f; // Assumes EffectProcRate is meaningful (e.g., chance per hit)

        float utilityScore = (consistencyScore * 0.5f) + (effectScore * 0.5f);

        // Calculate final weighted score using weights from TowerRatingWeights
        // Note: The meaning of 'costEfficiencyWeight' has changed to 'damageEfficiencyWeight (overkill)'
        float finalScore =
            (combatScore * weights.combatWeight) +
            (survivalScore * weights.survivabilityWeight) +
            (efficiencyScore * weights.costEfficiencyWeight) + // This weight now applies to the overkill penalty score
            (utilityScore * weights.utilityWeight);

        return Mathf.Clamp(finalScore, 0f, 100f);
    }

    private float NormalizeValueAgainstBaseline(float value, float baseline, float topValue)
    {
        // Handles potential division by zero or invalid ranges
        if (baseline <= 0 || topValue <= baseline)
        {
            // Simple linear scale from 0 to topValue mapped to 0-100
            return Mathf.Clamp01(value / (topValue > 0 ? topValue : 1f)) * 100f;
        }


        // Maps a value to 0-100 range where baseline = 50 and topValue = 100
        if (value <= 0)
            return 0;

        if (value >= topValue)
            return 100;

        if (value <= baseline)
        {
            // Scale linearly from 0 to 50 as value goes from 0 to baseline
            return (value / baseline) * 50f;
        }
        else
        {
            // Scale linearly from 50 to 100 as value goes from baseline to topValue
            return 50f + (50f * ((value - baseline) / (topValue - baseline)));
        }
    }
}
/// <summary>
/// Handles writing metrics data to CSV format
/// </summary>
public class CSVWriter
{
    public void WriteMetricsToCSV(List<TowerPerformanceMetrics> metricsList, string filePath)
    {
        try
        {
            StringBuilder csvContent = new StringBuilder();

            csvContent.AppendLine("TowerName,Level,Cost,DPS_Avg,TotalDamage,Kills,EffectiveDPS,OverkillPercentage,PeakDPS," + 
                                    "Accuracy,AOEHits_Avg,EffectProcRate,HP,MaxHP,TimeAlive,OverkillDamage,AttackConsistency,TowerRating"); 

            foreach (var metrics in metricsList)
            {
                csvContent.AppendLine(
                    $"{metrics.TowerName},{metrics.Level},{metrics.Cost},{metrics.AverageDPS:F2},{metrics.TotalDamage:F0}," +
                    $"{metrics.Kills},{metrics.EffectiveDPS:F2},{metrics.OverkillPercentage:F3},{metrics.PeakDPS:F2}," + 
                    $"{metrics.Accuracy:F3},{metrics.AverageAOEHits:F2},{metrics.EffectProcRate:F3},{metrics.Health:F0}," +
                    $"{metrics.MaxHealth:F0},{(float.IsPositiveInfinity(metrics.TimeAlive) ? "Inf" : metrics.TimeAlive.ToString("F1"))}," +
                    $"{metrics.OverkillDamage:F0},{metrics.AttackConsistency:F3},{metrics.TowerRating:F1}" 
                );
            }

            // Write to file
            File.WriteAllText(filePath, csvContent.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"[Tower Test] Failed to write CSV: {e.Message}");
        }
    }
}

/// <summary>
/// Weights for calculating tower rating
/// </summary>
public class TowerRatingWeights
{
    public float combatWeight = 0.4f;
    public float survivabilityWeight = 0.2f;
    public float costEfficiencyWeight = 0.3f;
    public float utilityWeight = 0.1f;

    public TowerRatingWeights() { }

    public TowerRatingWeights(float combat, float survival, float cost, float utility)
    {
        combatWeight = combat;
        survivabilityWeight = survival;
        costEfficiencyWeight = cost;
        utilityWeight = utility;

        // Normalize weights to ensure they sum to 1
        float sum = combatWeight + survivabilityWeight + costEfficiencyWeight + utilityWeight;
        if (sum != 0 && sum != 1f)
        {
            combatWeight /= sum;
            survivabilityWeight /= sum;
            costEfficiencyWeight /= sum;
            utilityWeight /= sum;
        }
    }
}

#endregion

