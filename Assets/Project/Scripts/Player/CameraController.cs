using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float heightOffset = 0.6f;
    [SerializeField] private float lookAheadOffset = 2.0f;
    [SerializeField] private Vector2 followTime = new(0.2f, 0.1f);

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 trackingPosition = Vector3.zero;


    void Update()
    {
        
    }


    private void UpdateFollow()
    {

    }
}
