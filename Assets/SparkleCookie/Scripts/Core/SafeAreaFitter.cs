// Sparkle Cookie - custom code.
using UnityEngine;

namespace SparkleCookie.Core
{
    /// <summary>
    /// Resizes a RectTransform to the device safe area so the top HUD clears
    /// notches/punch-holes on modern phones. Put this on a full-screen child of
    /// the Canvas and parent HUD elements under it. Re-applies on orientation /
    /// resolution change (the game is portrait-locked, but tablets vary).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea ||
                Screen.width != lastScreen.x ||
                Screen.height != lastScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            var safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreen = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            var min = safe.position;
            var max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
