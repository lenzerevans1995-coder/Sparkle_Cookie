// Sparkle Cookie - custom code (c) project authors. MIT for our code; see docs/README.md.
using System;

namespace SparkleCookie.Core
{
    /// <summary>
    /// Decoupled static event bus that bridges the licensed match-3 engine
    /// (GameVanilla.*) to our custom systems without the engine needing a hard
    /// reference to our types. The engine raises these via two tiny, documented
    /// edits (see docs/KIT_MODIFICATIONS.md); our systems subscribe.
    /// </summary>
    public static class SparkleHooks
    {
        /// <summary>Raised every time a tap-match successfully clears a group of
        /// animals. <c>count</c> = animals cleared, <c>score</c> = score awarded.</summary>
        public static event Action<int, int> MatchResolved;

        /// <summary>Raised when the Sparkle meter fires a board-wide burst
        /// (UI/FX listeners).</summary>
        public static event Action BurstTriggered;

        public static void RaiseMatchResolved(int count, int score) => MatchResolved?.Invoke(count, score);

        public static void RaiseBurstTriggered() => BurstTriggered?.Invoke();

        /// <summary>Clears all subscribers. Call on scene teardown to avoid leaks
        /// across play sessions in the editor.</summary>
        public static void Clear()
        {
            MatchResolved = null;
            BurstTriggered = null;
        }
    }
}
