using UnityEngine;
using static TowerData;

public class Main : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string towerName = "TestTowerOne";
    private Tower tower;
    void Start()
    {
        TestDummy testDummy = GameObject.Find("TestDummy").GetComponent<TestDummy>();
        TowerDefenseManager.Instance.RegisterTarget(testDummy);

        // Test New Tower
        TowerFactory.CreateTowerData(towerName, TowerLevel.LevelOne, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, null);
        TowerFactory.CreateTowerData(towerName, TowerLevel.LevelTwo, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, null);
        TowerFactory.CreateTowerData(towerName, TowerLevel.LevelThree, 1200, 1200, 2500, 800, 0.8f, 22f, 10f, null);
        string towerPrefabPath = "Prefabs/" + towerName;
        GameObject testTower = Resources.Load<GameObject>(towerPrefabPath);

        Vector3 pos = new Vector3(0, 0, 0);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, pos.normalized);
        tower = TowerUtils.SpawnTower(pos, rotation, TowerFactory.GetTowerData(towerName, TowerData.TowerLevel.LevelOne), testTower);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
