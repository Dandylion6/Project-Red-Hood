using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IParryable
{
    public float Health => health;

    private float health = 100.0f;


    public void TakeDamage(float damage)
    {
        health -= damage;
    }


    public bool CanParry()
    {
        return true;
    }


    public void Parry()
    {
        throw new System.NotImplementedException();
    }
}
