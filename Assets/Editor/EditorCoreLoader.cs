using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static public class EditorCoreLoader
{
    static EditorCoreLoader()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }


    static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isEditor) return;

        if (scene.name == "CoreScene")
        {
            return;
        }

        // Loads the core scene for any other scene additively
        SceneManager.LoadScene("CoreScene", LoadSceneMode.Additive);
        SceneManager.sceneLoaded -= OnSceneLoad; // Only happens once.
    }
}
