using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public GameObject firingSmoke;

    public float projectileSpeed = 5f;

    private float lastFireTime = -Mathf.Infinity;

    public void Attack(FiringTarget target, Transform firePoint, float fireRate, float damage)
    {
        if (Time.time >= lastFireTime + fireRate)
        {
            lastFireTime = Time.time;

            GameObject projectile = Projectile.InstanciateProjectileWithPossibleParent(this.transform, projectilePrefab, firePoint.position);

            projectile.GetComponent<Projectile>().Initialize(target, projectileSpeed, damage);

            Instantiate(firingSmoke, firePoint.transform.position, Quaternion.identity, transform);
        }
    }
}
