using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement movement = null;

    [Header("Follow Settings")]
    [SerializeField] private Transform target = null;
    [SerializeField] private bool staticFollowDistance = false;
    [SerializeField] [Tooltip("The distance from the target when idle; will be default if static follow.")] private float closeFollowDistance = 9.0f;
    [SerializeField] [Tooltip("The distance from the target when moving; will be ignored if static follow.")] private float farFollowDistance = 12.0f;
    [SerializeField] private float heightOffset = 0.6f;
    [SerializeField] private float lookAheadOffset = 2.0f;
    [SerializeField] private float moveSpeedMultiplier = 0.5f;
    [SerializeField] private Vector3 followTime = new(0.2f, 0.1f, 0.1f);
    [SerializeField] private float followDistanceTransitionTime = 3.0f;


    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 trackingPosition = Vector3.zero;
    private float distanceVelocity = 0.0f;
    private float currentFollowDistance = 0.0f;


    private void Start()
    {
        currentFollowDistance = closeFollowDistance;
        trackingPosition = GetFollowTracking();
    }


    void Update()
    {
        if (!staticFollowDistance)
        {
            float targetDistance = movement.Velocity.magnitude >= float.Epsilon ? farFollowDistance : closeFollowDistance;
            currentFollowDistance = Mathf.SmoothDamp(currentFollowDistance, targetDistance, ref distanceVelocity, followDistanceTransitionTime);
        }

        trackingPosition = GetFollowTracking();
        UpdateFollow();
    }


    private void UpdateFollow()
    {
        // Smoothly move the camera towards the tracking position with each axis separated.
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.SmoothDamp(currentPosition.x, trackingPosition.x, ref currentVelocity.x, followTime.x);
        currentPosition.y = Mathf.SmoothDamp(currentPosition.y, trackingPosition.y, ref currentVelocity.y, followTime.y);
        currentPosition.z = Mathf.SmoothDamp(currentPosition.z, trackingPosition.z, ref currentVelocity.z, followTime.z);
        transform.position = currentPosition;
    }


    private Vector3 GetFollowTracking()
    {
        Vector3 tracking = target.position;
        float lookAhead = lookAheadOffset + movement.CurrentMoveSpeed * moveSpeedMultiplier;

        tracking += target.forward * lookAhead;
        tracking += Vector3.up * heightOffset;
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
