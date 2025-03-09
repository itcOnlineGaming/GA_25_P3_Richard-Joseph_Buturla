using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : Tower
{
    private IAttackBehaviour attackStrategy;
    private Enemy targetEnemy;

    public GameObject Turret;

    public override void Initialise(TowerData data)
    {
        towerData = data;
        currentHealth = data.MaxHealth;
        attackStrategy = GetComponent<IAttackBehaviour>();
        range = data.Range;
    }

    public override void Update()
    {
        if (!placed) { return; }

        FindTarget();
        if (targetEnemy != null && attackStrategy != null)
        {
            attackStrategy.Attack(targetEnemy.transform, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEnemy.transform);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void LookAtTarget(Transform targetPos)
    {
        Vector3 aimVector;

        aimVector = targetPos.position - Turret.transform.position; //vector to player pos
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); //project it on an x-z plane 

        Quaternion q_penguin = Quaternion.LookRotation(aimVector, Turret.transform.up);


        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, q_penguin, 10f * Time.deltaTime); //slowly transition
    }


    void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float shortestDistance = range;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        if (targetEnemy == null)
        {
            targetEnemy = nearestEnemy;
        }
    }
}
