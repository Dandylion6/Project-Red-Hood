using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public enum Type
    {
        Follow,
        Region,
    }


    [Header("References")]
    [SerializeField] private PlayerMovement movement = null;

    [Header("Follow Settings")]
    [SerializeField] private Transform target = null;
    [SerializeField] private bool staticFollowDistance = false;
    [SerializeField] [Tooltip("The distance from the target when idle; will be default if static follow.")] private float closeFollowDistance = 9.0f;
    [SerializeField] [Tooltip("The distance from the target when moving; will be ignored if static follow.")] private float farFollowDistance = 12.0f;
    [SerializeField] [Tooltip("The time it takes to change follow distance; will be ignored if static follow.")] private float followDistanceTransitionTime = 3.0f;
    [SerializeField] private float heightOffset = 0.6f;
    [SerializeField] [Min(0.0f)] private float lookAheadOffset = 2.0f;
    [SerializeField] [Tooltip("How much move speed affects look ahead; high value means camera will respond more to movement.")] private Vector2 movementFollowMultiplier = Vector2.one;
    [SerializeField] [Tooltip("How much time it takes to get to the camera target position; lower value means faster responses.")] private Vector3 followTime = new(0.2f, 0.1f, 0.1f);

    [Header("Transition Settings")]
    [SerializeField] private Vector3 toRegionTime = new(0.4f, 0.2f, 0.2f);


    public CameraRegion ActiveRegion => activeRegion;

    private CameraRegion activeRegion = null;
    private Type currentType = Type.Follow;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 trackingPosition = Vector3.zero;
    private float distanceVelocity = 0.0f;
    private float currentFollowDistance = 0.0f;


    public void AttachCameraRegion(CameraRegion region)
    {
        activeRegion = region;
        currentType = Type.Region;
    }


    public void DetachCameraRegion()
    {
        currentType = Type.Follow;
        activeRegion = null;
    }


    private void Start()
    {
        currentFollowDistance = closeFollowDistance;
        trackingPosition = GetFollowTracking();
    }


    private void Update()
    {
        switch (currentType)
        {
            case Type.Follow: UpdateFollow();
                break;
            case Type.Region: UpdateRegion(); 
                break;

        }

        // Smoothly move the camera towards the tracking position with each axis separated.
        Vector3 currentPosition = transform.position;

        Vector3 currentSmoothTime = currentType == Type.Region ? toRegionTime : followTime;
        currentPosition.x = Mathf.SmoothDamp(currentPosition.x, trackingPosition.x, ref currentVelocity.x, currentSmoothTime.x);
        currentPosition.y = Mathf.SmoothDamp(currentPosition.y, trackingPosition.y, ref currentVelocity.y, currentSmoothTime.y);
        currentPosition.z = Mathf.SmoothDamp(currentPosition.z, trackingPosition.z, ref currentVelocity.z, currentSmoothTime.z);

        transform.position = currentPosition;
    }


    private void UpdateFollow()
    {
        if (!staticFollowDistance)
        {
            float targetDistance = movement.Velocity.magnitude >= float.Epsilon ? farFollowDistance : closeFollowDistance;
            currentFollowDistance = Mathf.SmoothDamp(currentFollowDistance, targetDistance, ref distanceVelocity, followDistanceTransitionTime);
        }

        trackingPosition = GetFollowTracking();
    }


    private void UpdateRegion()
    {
        trackingPosition = activeRegion.CameraPosition;
    }


    private Vector3 GetFollowTracking()
    {
        Vector3 tracking = target.position;
        Vector2 movementVelocity = movementFollowMultiplier * movement.Velocity;

        // Applies offsets and uses velocity for better look ahead.
        tracking += target.forward * (lookAheadOffset + Mathf.Abs(movementVelocity.x));
        tracking += Vector3.up * (heightOffset + movementVelocity.y);
        tracking.z = -currentFollowDistance;

        return tracking;
    }


    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(trackingPosition, 0.2f);
    }
}
