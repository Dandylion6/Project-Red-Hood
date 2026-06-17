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
    [SerializeField] [Tooltip("The name of the scene to teleport to.")] private string nextSceneName = "Next Scene";


    private void Awake()
    {
        // If the player just left the scene that this teleporter leads to, teleport them here.
        bool shouldTeleportHere = lastSceneName == nextSceneName;
        if (!shouldTeleportHere) return;
        GameManager.Instance.Player.position = transform.position;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        lastSceneName = GetCurrentSceneName();
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Additive);
    }


    private string GetCurrentSceneName()
    {
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            // Ignores the CoreScene, which is always loaded in gameplay scenes.
            if (scene.isLoaded && scene.name != "CoreScene")
            {
                return scene.name;
            }
        }
        return string.Empty;
    }
}
