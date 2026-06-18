using UnityEngine;

[RequireComponent(typeof(Stunnable))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Base Settings")]
    [SerializeField] private float maxHealth = 100.0f;


    public float MaxHealth => maxHealth;
    public float Health => health;
   
    protected Stunnable Stunnable => stunnable;


    private Stunnable stunnable = null;
    private float health = 100.0f;


    public void TakeDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0.0f);
        if (health <= float.Epsilon) Die();
    }


    public virtual void Die()
    {
        Destroy(gameObject);
    }


    protected abstract bool CanParry();

    protected abstract bool CanAttack();

    protected abstract void Attack();


    private void Start()
    {
        stunnable = GetComponent<Stunnable>();
        stunnable.SubscribeToCanStun(CanParry);
    }
}
