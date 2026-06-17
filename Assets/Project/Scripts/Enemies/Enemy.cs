using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IParryable
{
    public float Health => health;

    private float health = 100.0f;


    public void TakeDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0.0f);
        if (health <= float.Epsilon) Die();
    }


    public void Die()
    {
        Destroy(gameObject);
    }


    public bool CanParry()
    {
        return true;
    }


    public void Parry()
    {
        Debug.Log("Enemy parried!");
    }
}
