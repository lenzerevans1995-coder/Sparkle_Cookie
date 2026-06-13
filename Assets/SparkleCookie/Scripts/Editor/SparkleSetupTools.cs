// Sparkle Cookie - custom editor tooling.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SparkleCookie.Board;

namespace SparkleCookie.EditorTools
{
    /// <summary>
    /// Repeatable, documented wiring of our custom playable board into the open
    /// scene (the GUI-pack demo Game-Portrait). Adds a SparkleBoard controller that
    /// takes over the demo's "Table" grid at runtime. Run from the "Sparkle Cookie"
    /// menu after opening the Game scene.
    /// </summary>
    public static class SparkleSetupTools
    {
        [MenuItem("Sparkle Cookie/Install Playable Board Into Open Scene")]
        public static void InstallBoard()
        {
            if (GameObject.Find("Canvas/Table") == null)
            {
                EditorUtility.DisplayDialog("Sparkle Cookie",
                    "No 'Canvas/Table' found. Open the Game-Portrait based scene first.", "OK");
                return;
            }

            var root = GameObject.Find("SparkleBoard");
            if (root == null) root = new GameObject("SparkleBoard");
            if (root.GetComponent<SparkleBoard>() == null) root.AddComponent<SparkleBoard>();

            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[SparkleCookie] SparkleBoard installed. It auto-finds the Table " +
                      "and HUD by hierarchy path at runtime.");
        }
    }
}
#endif
