// Sparkle Cookie - custom code.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameVanilla.Game.Common;
using SparkleCookie.Board;

namespace SparkleCookie.Core
{
    /// <summary>
    /// Bridges the GUI-pack's demo level buttons to the PuzzleMatchKit engine:
    /// on click it records the chosen level on the persistent
    /// <see cref="PuzzleMatchManager"/> and loads the Game scene. Attach to each
    /// demo "Level Button - N" (the editor setup populates <see cref="level"/>
    /// from the button name).
    /// </summary>
    public class SparkleLevelLoader : MonoBehaviour
    {
        public int level = 1;
        [Tooltip("Highest level JSON that actually exists in Resources/Levels.")]
        public int maxAvailableLevel = 50;
        public string gameScene = "Game";

        private void Start()
        {
            var btn = GetComponent<Button>();
            if (btn == null) btn = GetComponentInChildren<Button>(true);
            if (btn != null) btn.onClick.AddListener(Load);
        }

        public void Load()
        {
            var chosen = Mathf.Clamp(level, 1, maxAvailableLevel);
            SparkleSession.SelectedLevel = chosen;
            if (PuzzleMatchManager.instance != null)
            {
                PuzzleMatchManager.instance.lastSelectedLevel = chosen;
            }
            SceneManager.LoadScene(gameScene);
        }
    }
}
