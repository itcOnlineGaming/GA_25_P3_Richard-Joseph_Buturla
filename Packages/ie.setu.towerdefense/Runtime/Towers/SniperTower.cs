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
            attackStrategy.Attack(targetEntity.Transform, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEntity.Transform);
        }
    }

    void LookAtTarget(Transform targetPos)
    {
        if (targetPos == null) return;

        // 'Up' direction relative to planet
        Vector3 planetUp = (firePoint.position - planetCenter).normalized;

        // Direction to target
        Vector3 aimVector = targetPos.position - firePoint.position;

        // Project onto planet surface
        Vector3 horizontalAim = Vector3.ProjectOnPlane(aimVector, planetUp);
        if (horizontalAim.sqrMagnitude < 0.001f)
            horizontalAim = firePoint.forward;

        // Compute yaw and pitch
        Quaternion yawRotation = Quaternion.LookRotation(horizontalAim, planetUp);
        Vector3 turretRight = yawRotation * Vector3.right;
        float pitchAngle = Vector3.SignedAngle(horizontalAim, aimVector, turretRight);
        Quaternion pitchRotation = Quaternion.AngleAxis(pitchAngle, turretRight);

        // Apply rotations
        Quaternion targetRotation = pitchRotation * yawRotation;
        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, targetRotation, 10f * Time.deltaTime);
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
