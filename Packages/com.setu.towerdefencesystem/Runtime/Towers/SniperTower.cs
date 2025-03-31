using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private ITargetable targetEntity;

    public GameObject Turret;
    public float cutOffRange = 5.0f;

    private Vector3 planetCenter = Vector3.zero; // Assuming the planet is at (0,0,0)

    public override void Initialise(TowerData data)
    {
        base.Initialise(data);
        attackStrategy = GetComponent<IAttackBehaviour>();
        range = data.Range;
    }

    public override void Update()
    {
        if (!placed) return;

        FindTarget();
        if (targetEntity != null && attackStrategy != null)
        {
            FiringTarget firingTarget = new FiringTarget(targetEntity.Transform);
            attackStrategy.Attack(firingTarget, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEntity.Transform);
        }

        if (currentHealth <= 0)
        {
            TowerDefenseEvents.RaiseTowerDestroyed(this);
            GameObject.Destroy(gameObject);
        }
    }

    void LookAtTarget(Transform targetPos)
    {
        if (targetPos == null) return;

        Vector3 aimVector;

        aimVector = targetPos.position - Turret.transform.position; //vector to target pos
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); //project it on an x-z plane 

        Quaternion q_penguin = Quaternion.LookRotation(aimVector, Turret.transform.up);

        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, q_penguin, 10f * Time.deltaTime); //slowly transition
    }

    void FindTarget()
    {
        List<ITargetable> targets = TowerDefenseManager.Instance.GetAllTargets();
        float shortestDistance = Mathf.Infinity;
        ITargetable nearestTarget = null;

        foreach (ITargetable target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            float distance = Vector3.Distance(transform.position, target.Position);

            // Ensure target is within range and outside cutoff range
            if (distance < shortestDistance && distance <= range && distance > cutOffRange)
            {
                shortestDistance = distance;
                nearestTarget = target;
            }
        }

        // If no target or previous target is invalid, assign the nearest valid one
        if (targetEntity == null || !targetEntity.IsAlive)
        {
            targetEntity = nearestTarget;
        }

        // Drop target if it moves too close
        if (targetEntity != null && Vector3.Distance(transform.position, targetEntity.Position) <= cutOffRange)
        {
            targetEntity = null;
        }
    }
}
