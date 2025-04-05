using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroPlaneTower : Tower
{
    public GameObject planePrefab; 
    private GameObject spawnedPlane;
    private Vector3 normal;

    public override void Initialise(TowerData data)
    {
        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }
        towerData = data;
        currentHealth = data.MaxHealth;
        normal = transform.up;
        if(placed)
        {
            SpawnPlane();
        }
        
    }

    void SpawnPlane()
    {
        if (spawnedPlane == null)
        {
            spawnedPlane = Instantiate(planePrefab, transform.position + normal * 5f, Quaternion.identity);

            // Make the plane a child of the parent of this tower. Usually "Planet" here
            spawnedPlane.transform.SetParent(transform.parent, true);
        }

        AeroPlaneUnit planeUnit = spawnedPlane.GetComponent<AeroPlaneUnit>();
        if (planeUnit != null)
        {
            planeUnit.Initialise(this, towerData, normal);
        }
    }


    public override void Update()
    {
        if (!placed) return;

        if (currentHealth <= 0)
        {
            TowerDefenseEvents.RaiseTowerDestroyed(this);
            GameObject.Destroy(gameObject);
            GameObject.Destroy(spawnedPlane);
        }
    }
}
