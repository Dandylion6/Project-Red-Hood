using UnityEngine;

/// <summary>
/// Assign to a UI element that should only be visible on mobile devices. The element will be hidden on non-mobile platforms.
/// </summary>
public class MobileUI : MonoBehaviour
{
    private void Awake()
    {
        // Will set the element as inactive while not on mobile and isn't in the editor (relevant for testing.)
        if (!Application.isMobilePlatform && !Application.isEditor)
        {
            gameObject.SetActive(false);
        }
    }
}
