using UnityEngine;

public class CameraRegion : MonoBehaviour
{
    [Header("Region Settings")]
    [SerializeField] private float regionWidth = 10.0f;


    public Vector3 CameraPosition => cameraPosition;

    private Vector3 cameraPosition = Vector3.zero;


    void Start()
    {
        Camera dummyCamera = GetComponentInChildren<Camera>();
        SetCameraData(dummyCamera);
        Destroy(dummyCamera.gameObject); // Remove clutter.
    }


    private void SetCameraData(Camera camera)
    {
        cameraPosition = camera.transform.position;
    }


    void Update()
    {
        bool isCurrentlyActive = CameraController.Instance.ActiveRegion == this;
        bool shouldBeActive = IsPlayerInRegion();

        if (!isCurrentlyActive && shouldBeActive)
        {
            CameraController.Instance.AttachCameraRegion(this);
        }
        else if (isCurrentlyActive && !shouldBeActive)
        {
            CameraController.Instance.DetachCameraRegion();
        }
    }


    private bool IsPlayerInRegion()
    {
        Transform player = GameManager.Instance.Player;
        float extent = regionWidth * 0.5f;

        if (player.position.x < transform.position.x - extent) return false;
        if (player.position.x > transform.position.x + extent) return false;
        return true;
    }


    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        position.z = 0.0f;
        transform.position = position;

        DrawRegion();
    }


    private void DrawRegion()
    {
        Gizmos.color = Color.white;
        Vector3 leftSide = transform.position + 0.5f * regionWidth * Vector3.left;
        Gizmos.DrawLine(leftSide, leftSide + Vector3.up * 100.0f);

        Vector3 rightSide = transform.position + 0.5f * regionWidth * Vector3.right;
        Gizmos.DrawLine(rightSide, rightSide + Vector3.up * 100.0f);
    }
}
