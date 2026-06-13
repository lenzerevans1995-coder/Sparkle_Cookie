// Sparkle Cookie - custom code.
using UnityEngine;

namespace SparkleCookie.Companions
{
    public enum Rarity { Common, Rare, Epic, Legendary }

    /// <summary>
    /// A collectible animal character the player can unlock and equip. Today it is
    /// represented by a 2D portrait from the GUI pack; <see cref="modelPrefab"/> is
    /// the reserved slot for the future 3D model (see docs/ROADMAP_3D_ANIMALS.md).
    /// Create instances via Assets > Create > SparkleCookie > Animal Companion.
    /// </summary>
    [CreateAssetMenu(fileName = "Companion", menuName = "SparkleCookie/Animal Companion")]
    public class AnimalCompanion : ScriptableObject
    {
        public string id;
        public string displayName;
        public Rarity rarity = Rarity.Common;
        [TextArea] public string description;

        [Header("Presentation")]
        public Sprite portrait2D;            // used now (GUI-pack art)
        public GameObject modelPrefab;       // 3D model slot - roadmap

        [Header("Economy")]
        public int unlockCostCoins = 500;
        public int unlockCostStars = 0;

        [Header("Gameplay")]
        [Tooltip("Maps to an ICompanionPower implementation. Empty = cosmetic only.")]
        public string powerId;
    }
}
