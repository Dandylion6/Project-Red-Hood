using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger volume that loads <see cref="nextSceneName"/> when the player enters it.
/// Teleporters are bidirectional without needing per-scene spawn data: each one checks
/// on Awake whether its own destination matches the scene the player just left, and if
/// so, treats itself as the matching teleporter on this side and moves the player there.
/// Assumes CoreScene (and GameManager) stay loaded persistently across the additive
/// scene swap.
/// </summary>
public class Teleporter : MonoBehaviour
{
    // Tracks the scene the player just left. Static so it survives the scene unload/load that happens during a teleport.
    static private string lastSceneName = string.Empty;


    [Header("Teleporter Settings")]
    [SerializeField] [Tooltip("The name of the scene to teleport to.")] private string currentSceneName = "Current Scene";
    [SerializeField] [Tooltip("The name of the scene to teleport to.")] private string nextSceneName = "Next Scene";
    [SerializeField] private float teleportCooldown = 1.0f;
    [SerializeField] private Vector3 playerSpawnOffset = Vector3.zero;

    private float lastTeleportTime = 0.0f;


    private void Awake()
    {
        lastTeleportTime = Time.time - teleportCooldown; // Allow immediate teleportation on first load.

        // If the player just left the scene that this teleporter leads to, teleport them here.
        bool shouldTeleportHere = lastSceneName == nextSceneName;
        if (!shouldTeleportHere) return;

        lastTeleportTime = Time.time;
        GameManager.Instance.Player.position = transform.position + playerSpawnOffset;
        GameManager.Instance.Player.gameObject.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        float timeSinceLastTeleport = Time.time - lastTeleportTime;
        if (timeSinceLastTeleport < teleportCooldown) return;

        GameManager.Instance.Player.gameObject.SetActive(false); // Deactivate player to avoid issues during scene transition)
        lastSceneName = currentSceneName;

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(lastSceneName);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + playerSpawnOffset, 0.5f);
    }
}
