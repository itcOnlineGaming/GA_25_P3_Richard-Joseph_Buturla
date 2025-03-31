using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarProjectile : Projectile
{
    public float aoeRadius = 1f;
    public GameObject explosion;

    [Header("Flight Parameters")]
    public float initialAscentTime = 0.5f; // Time to spend in initial upward flight
    public float ascentHeight = 10f; // How high to rise during initial ascent

    private Vector3 initialPosition;
    private float elapsedTime = 0f;
    private bool hasLaunched = false;
    private bool isAscending = true; // Tracks if we're in the initial ascent phase

    public override void Initialize(FiringTarget target, float speed, float damage)
    {
        base.Initialize(target, speed, damage);
        initialPosition = transform.position;
        hasLaunched = true;
    }

    void Update()
    {
        if (!hasLaunched) return;

        if (target.GetTargetPosition() == null)
        {
            Destroy(gameObject);
            return;
        }


        UpdateProjectile();
    }

    protected override void UpdateProjectile()
    {
        elapsedTime += Time.deltaTime;

        if (isAscending)
        {
            UpdateAscent();
        }
        else
        {
            UpdateTargeting();
        }
    }

    void UpdateAscent()
    {
        Vector3 localUp = (transform.position - planetCenter).normalized;
        float ascentProgress = Mathf.Clamp01(elapsedTime / initialAscentTime);
        Vector3 moveVector = localUp * ascentHeight * Time.deltaTime / initialAscentTime;

        transform.position += moveVector;
        transform.LookAt(transform.position + moveVector);

        if (ascentProgress >= 1.0f)
        {
            isAscending = false;
            elapsedTime = 0f;
        }
    }

    void UpdateTargeting()
    {
        Vector3 direction = (target.GetTargetPosition() - transform.position).normalized;
        Vector3 gravity = (planetCenter - transform.position).normalized * gravityEffect;
        Vector3 moveVector = (direction + gravity).normalized * speed * Time.deltaTime;

        transform.position += moveVector;
        transform.LookAt(transform.position + moveVector);
    }

    void OnTriggerEnter(Collider other)
    {
        ITargetable targetable = other.GetComponent<ITargetable>();

         if (targetable != null)
        {
            targetable.TakeDamage(damage);
            OnImpact();
            return;
        }

        if (other.CompareTag("Triangle")) // Ignore for now
        {
            OnImpact();
        }
    }

    void OnImpact()
    {
        Vector3 normal = (transform.position - planetCenter).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);

        GameObject planet = GameObject.Find("Planet");
        if(planet != null)
        {
            GameObject explosionInstance = Instantiate(explosion, transform.position, rotation, planet.transform);

            foreach (Transform child in explosionInstance.transform)
            {
                child.rotation = Quaternion.LookRotation(normal);
            }
        }
        

        ApplyAOEDamage();

        Destroy(gameObject);
    }

    void ApplyAOEDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (Collider col in hitColliders)
        {
            ITargetable targetable = col.GetComponent<ITargetable>();
            if (targetable != null)
            {
                targetable.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
