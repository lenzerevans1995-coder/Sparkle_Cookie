// Sparkle Cookie - custom code.
namespace SparkleCookie.Board
{
    /// <summary>
    /// Tiny static carrier for cross-scene state (which level the player picked on
    /// the Level screen). Avoids any hard dependency on the licensed kit's manager
    /// for the playable board.
    /// </summary>
    public static class SparkleSession
    {
        public static int SelectedLevel = 1;
    }
}
