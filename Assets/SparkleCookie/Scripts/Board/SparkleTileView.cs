// Sparkle Cookie - custom code.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SparkleCookie.Board
{
    /// <summary>
    /// View for a single persistent board cell. Built once from a GUI-pack animal
    /// Tile prefab (so the framed-cell look is exactly the demo's); its animal is
    /// then swapped in place by <see cref="SetType"/>. Taps are handled by
    /// <see cref="SparkleBoard"/> via direct raycast (not the UI Button), so this
    /// view only stores its grid coordinates and animates.
    /// </summary>
    public class SparkleTileView : MonoBehaviour
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int Type { get; private set; }

        private Image itemImage;

        public void Init(int r, int c)
        {
            Row = r;
            Col = c;

            var item = transform.Find("Button/Item");
            if (item != null) itemImage = item.GetComponent<Image>();
            if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

            // Disable the idle Animator that keyframes the sprite, so our per-type
            // animal assignment sticks.
            if (itemImage != null)
            {
                var an = itemImage.GetComponent<Animator>();
                if (an != null) an.enabled = false;
            }
        }

        public void SetType(int type, Sprite sprite, bool pop)
        {
            Type = type;
            if (itemImage != null && sprite != null) itemImage.sprite = sprite;
            if (pop && isActiveAndEnabled)
            {
                StopAllCoroutines();
                StartCoroutine(PopRoutine());
            }
        }

        private IEnumerator PopRoutine()
        {
            var t = itemImage != null ? itemImage.transform : transform;
            const float dur = 0.18f;
            float e = 0f;
            Vector3 from = Vector3.one * 0.5f;
            Vector3 to = Vector3.one;
            t.localScale = from;
            while (e < dur)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / dur);
                // ease-out-back
                const float s = 1.70158f;
                float km1 = k - 1f;
                float eased = 1f + (s + 1f) * km1 * km1 * km1 + s * km1 * km1;
                t.localScale = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
            t.localScale = to;
        }
    }
}
