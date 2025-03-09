using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GoldMineTower : Tower
{
    public UnityEvent onGoldCollected;

    public GameObject coinPopUp;
    public Transform spawnPoint;
    public float spawnInterval = 5f;

    [SerializeField]
    public ParticleSystem smoke1;
    public ParticleSystem smoke2;

    public bool broken = true;
    public override void Initialise(TowerData data)
    {
        towerData = data;
        currentHealth = 0; // Tower starts with 0 health
        //currentHealth = data.MaxHealth;
        smoke1.Stop();
        smoke2.Stop();
        targetable = false;
    }
    public override void Update()
    {
        if (!broken && currentHealth <= 0)
        {
            broken = true;
            StopCoroutine(GenerateResources());
        }
    }

    public override void RepairTower(int _amount)
    {
        currentHealth += _amount;
        if (currentHealth >= towerData.MaxHealth)
        {
            currentHealth = towerData.MaxHealth;
        }

        broken = false;
        targetable = true;
        Reset();
    }

    public void Reset()
    {
        if(!broken)
        {
            smoke1.Play();
            smoke2.Play();
            StartCoroutine(GenerateResources());
        }
    }
    public IEnumerator GenerateResources()
    {
        Debug.Log("HAHAHAHAHA");
        yield return new WaitForSeconds(spawnInterval);
        Instantiate(coinPopUp, spawnPoint.position, Quaternion.identity, spawnPoint.parent.transform);
    }

}

