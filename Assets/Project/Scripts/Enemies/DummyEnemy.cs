using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DummyEnemy : Enemy
{
    [Header("Dummy Settings")]
    [SerializeField] private float attackTime = 1.0f;
    [SerializeField] private float attackCooldown = 2.0f;

    private float attackTimeLeft = 0.0f;
    private float attackCooldownLeft = 0.0f;
    private bool isIdle = true;


    protected override bool CanParry()
    {
        if (isIdle) return false;
        return true;
    }


    protected override bool CanAttack()
    {
        if (Stunnable.IsStunned) return false;
        if (attackCooldownLeft > float.Epsilon) return false;
        return true;
    }


    protected override void Attack()
    {
        attackTimeLeft = attackTime;
        isIdle = false;
    }


    private void Update()
    {
        bool isAttacking = attackTimeLeft > float.Epsilon;
        if (!isAttacking && !isIdle)
        {
            attackCooldownLeft = attackCooldown;
            isIdle = true;
        } else if (isAttacking)
        {
            attackTimeLeft -= Time.deltaTime;
        }

        if (isIdle)
        {
            attackCooldownLeft -= Time.deltaTime;
            if (CanAttack()) Attack();
        }
    }


    private void OnDrawGizmos()
    {
        if (Stunnable == null) return;
        Color color = Stunnable.IsStunned ? Color.yellow : (isIdle ? Color.red : Color.green);
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position + Vector3.up * 1.5f, 0.6f);
    }
}
