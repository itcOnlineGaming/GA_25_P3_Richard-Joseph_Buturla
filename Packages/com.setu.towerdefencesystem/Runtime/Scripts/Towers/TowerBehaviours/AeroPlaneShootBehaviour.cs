using UnityEngine;

public class AeroPlaneShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    public GameObject firingSmoke;

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