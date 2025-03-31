using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public GameObject firingSmoke;

    public float projectileSpeed = 5f;

    private float lastFireTime = -Mathf.Infinity;

    public void Attack(Transform target, Transform firePoint, float fireRate, float damage)
    {
        if (Time.time >= lastFireTime + fireRate)
        {
            lastFireTime = Time.time;
            GameObject plane = GameObject.Find("Plane"); // was planet
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity, plane.transform);
            projectile.GetComponent<Projectile>().Initialize(target, projectileSpeed, damage);

            TowerDefenseEvents.RaiseProjectileFired();

            Instantiate(firingSmoke, firePoint.transform.position, Quaternion.identity, transform);
        }
    }
}
