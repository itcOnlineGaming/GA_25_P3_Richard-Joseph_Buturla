using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Transform target;
    protected float speed;
    protected float damage;
    protected Vector3 planetCenter = Vector3.zero; // Default to world origin
    protected float gravityEffect = 0.1f;
    protected int HitGoldGain = 1;

    public virtual void Initialize(Transform target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    protected virtual void UpdateProjectile()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 gravity = (planetCenter - transform.position).normalized * gravityEffect;

        Vector3 moveVector = (direction + gravity).normalized * speed * Time.deltaTime;
        transform.position += moveVector;

        transform.LookAt(transform.position + moveVector);
    }
}
