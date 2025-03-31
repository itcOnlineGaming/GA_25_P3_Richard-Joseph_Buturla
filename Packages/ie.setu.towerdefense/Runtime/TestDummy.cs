using UnityEngine;

public class TestDummy : MonoBehaviour, ITargetable
{
    [SerializeField] private float health = 100f;

    public Transform Transform => transform;

    public bool IsAlive => health > 0;

    public Vector3 Position => transform.position;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"TestDummy took {damage} damage. Remaining health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("TestDummy has been destroyed.");
        Destroy(this.gameObject);
    }
}
