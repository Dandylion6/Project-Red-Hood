using UnityEngine;

[RequireComponent(typeof(Stunnable))]
public class Enemy : MonoBehaviour, IDamageable
{
    public float Health => health;


    private Stunnable stunnable = null;
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


    private void Start()
    {
        stunnable = GetComponent<Stunnable>();
        stunnable.SubscribeToCanStun(CanParry);
    }


    private bool CanParry()
    {
        return true;
    }

}
