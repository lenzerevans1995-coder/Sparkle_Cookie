// Sparkle Cookie - custom code.
using UnityEngine;
using UnityEngine.UI;

namespace SparkleCookie.Board
{
    /// <summary>
    /// View for a single persistent board cell. Built once from a GUI-pack animal
    /// Tile prefab (so the framed-cell look is exactly the demo's); its animal is
    /// then swapped in place by <see cref="SetType"/> as the board changes. Relays
    /// taps to the <see cref="SparkleBoard"/>.
    /// </summary>
    public class SparkleTileView : MonoBehaviour
    {
        private SparkleBoard board;
        private int row, col;
        private Image itemImage;
        public int Type { get; private set; }

        public void Init(SparkleBoard owner, int r, int c)
        {
            board = owner;
            row = r;
            col = c;

            var item = transform.Find("Button/Item");
            if (item != null) itemImage = item.GetComponent<Image>();
            if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

            // The demo tile's Item has an idle Animator that keyframes the sprite
            // (eye-blink) and would override our per-type swap every frame. Disable
            // it so the animal we assign actually shows.
            if (itemImage != null)
            {
                var an = itemImage.GetComponent<Animator>();
                if (an != null) an.enabled = false;
            }

            var btn = GetComponentInChildren<Button>(true);
            if (btn == null)
            {
                if (GetComponent<Image>() == null) gameObject.AddComponent<Image>();
                btn = gameObject.AddComponent<Button>();
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }

        public void SetType(int type, Sprite sprite)
        {
            Type = type;
            if (itemImage != null && sprite != null) itemImage.sprite = sprite;
        }

        private void OnClick()
        {
            if (board != null) board.OnTileClicked(row, col);
        }
    }
}
