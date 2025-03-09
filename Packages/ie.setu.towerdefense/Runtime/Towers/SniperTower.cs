using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private Enemy targetEnemy;

    public GameObject Turret;

    private Vector3 planetCenter = Vector3.zero; // Assuming the planet is at (0,0,0)

    public float cutOffRange = 5.0f;

    public void Start()
    {
        
    }

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
    }


    void LookAtTarget(Transform targetPos)
    {
        // Determine the 'up' direction at the fire point on the planet.
        Vector3 planetUp = (firePoint.position - planetCenter).normalized;

        // Aim vector from the fire point to the target.
        Vector3 aimVector = targetPos.position - firePoint.position;

        // Project the aim vector onto the horizontal plane defined by planetUp.
        Vector3 horizontalAim = Vector3.ProjectOnPlane(aimVector, planetUp);
        if (horizontalAim.sqrMagnitude < 0.001f)
            horizontalAim = firePoint.forward;

        // Yaw: Get a rotation that aligns the forward direction with the horizontal aim.
        Quaternion yawRotation = Quaternion.LookRotation(horizontalAim, planetUp);

        // Compute the turret's right vector from the yaw rotation.
        Vector3 turretRight = yawRotation * Vector3.right;

        // Instead of using world Y, calculate pitch as the signed angle between the horizontalAim and the full aimVector.
        float pitchAngle = Vector3.SignedAngle(horizontalAim, aimVector, turretRight);

        // Create a pitch rotation about the turret's right axis.
        Quaternion pitchRotation = Quaternion.AngleAxis(pitchAngle, turretRight);

        // Combine yaw and pitch. Multiplying pitchRotation * yawRotation applies pitch relative to the yaw'd orientation.
        Quaternion targetRotation = pitchRotation * yawRotation;

        // Smoothly interpolate the turret's rotation to the target rotation.
        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            // Only consider enemies that are within range and not too close.
            if (distance < shortestDistance && distance <= range && distance > cutOffRange)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        // If there's no current target or the current one is dead, update to the nearest valid enemy.
        if (targetEnemy == null || targetEnemy.animationStat == AnimationState.Dead)
        {
            targetEnemy = nearestEnemy;
        }

        // If the current target moved into the cutOffRange, drop it.
        if (targetEnemy != null)
        {
            float distanceToTrackedEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);
            Debug.Log("Distance to tracked enemy: " + distanceToTrackedEnemy);

            if (distanceToTrackedEnemy <= cutOffRange)
            {
                targetEnemy = null;
                Debug.Log("swap targets");
            }
        }
    }



}
