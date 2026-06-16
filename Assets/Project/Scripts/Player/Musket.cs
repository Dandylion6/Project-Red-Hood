using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Musket : MonoBehaviour
{
    private class Projectile
    {
        public IDamageable[] penetratedTargets; // Track targets that have been hit so they don't get hit due to peneration multiple times.
        public Vector3 position = Vector3.zero;
        public Vector3 direction = Vector3.forward;
        public float distanceTraveled = 0.0f;
        public readonly int maxPenerations = 0;
        public int penetrationCount = 0;
        public bool shouldRemove = false;


        public Projectile(Vector3 position, Vector3 direction, int penetrations)
        {
            this.position = position;
            this.direction = direction;

            maxPenerations = penetrations;
            penetratedTargets = new IDamageable[penetrations];
        }
    }


    [Header("References")]
    [SerializeField] private Transform barrelTransform = null;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 1.0f;
    [SerializeField] private float baseDamage = 40.0f;
    [SerializeField] [Tooltip("The amount of speed reduction while firing in percentage.")] private float movementSpeedPenalty = 50.0f;
    [SerializeField] [Tooltip("Layers that the projectile will go through.")] private LayerMask projectileIgnoreLayers = new();

    [Header("Projectile Settings")]
    [SerializeField] private float projectileRange = 20.0f;
    [SerializeField] private float projectileSpeed = 30.0f;
    [SerializeField] private float projectileRadius = 0.1f;
    [SerializeField] [Tooltip("The number of times the projectile can penetrate targets")] private int penetration = 1;

    [Header("Parry Settings")]
    [SerializeField] [Tooltip("Multiplier for cooldown when no parry is performed")] private float noParryCooldownMultiplier = 1.5f;
    [SerializeField] [Tooltip("The time window during which a parry can be executed; a higher value is more forgiving.")] private float parryWindow = 0.5f;
    [SerializeField] [Tooltip("The range which the projectile will instantly travel in order to parry; afterward it will have normal travel speed.")] private float maxParryDistance = 2.0f;


    private readonly List<Projectile> projectiles = new();
    private float fireCooldownLeft = 0.0f;


    public void OnFireInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (CanFire()) Fire();
    }


    private bool CanFire()
    {
        if (fireCooldownLeft > 0.0f) return false;
        return true;
    }


    private void Fire()
    {
        // Before becoming a projectile, we first check if we can parry something.
        if (CanParry(out IParryable parryable))
        {
            OnParry(parryable);

            fireCooldownLeft = fireCooldown * noParryCooldownMultiplier; // Applies a cooldown multiplier for parries.

            if (parryable is not IDamageable damageable) return;
            damageable.TakeDamage(baseDamage);
            return;
        }

        // If nothing can be parried, we fire a projectile as normal.
        fireCooldownLeft = fireCooldown;
        Projectile newProjectile = new(barrelTransform.position, barrelTransform.forward, penetration);
        projectiles.Add(newProjectile);
    }


    private bool CanParry(out IParryable parryable)
    {
        parryable = null;
        if (Physics.SphereCast(barrelTransform.position, projectileRadius, barrelTransform.forward, out RaycastHit hit, maxParryDistance, ~projectileIgnoreLayers))
        {
            parryable = hit.collider.GetComponent<IParryable>();
            return true;
        }
        return false;
    }


    private void OnParry(IParryable parryable)
    {
        parryable.Parry();
    }


    private void Update()
    {
        for (int i = 0; i < projectiles.Count; ++i)
            UpdateProjectile(i);
    }


    private void UpdateProjectile(int index)
    {
        Projectile projectile = projectiles[index];

        float displacement = projectileSpeed * Time.deltaTime;
        Vector3 newPosition = projectile.position + displacement * projectile.direction;

        TryRegisterHit(projectile, displacement);

        projectile.position = newPosition;
        projectile.distanceTraveled += displacement;

        if (projectile.distanceTraveled >= projectileRange)
            projectile.shouldRemove = true;

        if (projectile.shouldRemove)
        {
            projectiles.RemoveAt(index);
            return;
        }
    }


    /// <summary>
    /// Checks if the projectile has hit any damageable targets and registers the hit if it has.
    /// </summary>
    private void TryRegisterHit(Projectile projectile, float displacement)
    {
        if (!Physics.SphereCast(projectile.position, projectileRadius, projectile.direction, out RaycastHit hit, displacement, ~projectileIgnoreLayers)) return;

        if (!hit.collider.TryGetComponent(out IDamageable damageable))
        {
            projectile.shouldRemove = true;
        }

        if (IsAlreadyHit(projectile, damageable)) return;

        damageable.TakeDamage(baseDamage);
        projectile.penetratedTargets[projectile.penetrationCount++] = damageable;

        // If the projectile has hit the maximum number of targets it can penetrate, mark it for removal.
        if (projectile.penetrationCount >= projectile.maxPenerations)
        {
            projectile.shouldRemove = true;
        }
    }


    private bool IsAlreadyHit(Projectile projectile, IDamageable damageable)
    {
        foreach (IDamageable hit in projectile.penetratedTargets)
        {
            if (hit == damageable) return true;
        }
        return false;
    }


    private void OnDrawGizmos()
    {
        if (barrelTransform == null) return;

        // Draws the parry range in front of the musket barrel.
        Vector3 parryRangePosition = barrelTransform.position + barrelTransform.forward * maxParryDistance;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(barrelTransform.position, parryRangePosition);
        Gizmos.DrawWireSphere(parryRangePosition, projectileRadius); // Shows the projectil radius.

        // Draws the projectile range in front of the musket barrel.
        Gizmos.color = Color.red;
        Gizmos.DrawLine(parryRangePosition, barrelTransform.position + barrelTransform.forward * projectileRange);
        Gizmos.DrawWireSphere(barrelTransform.position + barrelTransform.forward * projectileRange, projectileRadius);
    }
}
