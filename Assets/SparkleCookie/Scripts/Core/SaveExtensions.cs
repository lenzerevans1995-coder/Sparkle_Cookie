// Sparkle Cookie - custom code.
using System.Collections.Generic;
using UnityEngine;

namespace SparkleCookie.Core
{
    /// <summary>
    /// Our save data, kept in PlayerPrefs under "sc_" keys so it never collides
    /// with the engine's keys (num_coins, num_lives, next_level, etc.). Stores the
    /// collectible-companion meta state.
    /// </summary>
    public static class SaveExtensions
    {
        private const string OwnedKey = "sc_owned_companions";
        private const string EquippedKey = "sc_equipped_companion";

        public static List<string> GetOwnedCompanions()
        {
            var raw = PlayerPrefs.GetString(OwnedKey, string.Empty);
            return string.IsNullOrEmpty(raw)
                ? new List<string>()
                : new List<string>(raw.Split(','));
        }

        public static bool IsOwned(string companionId) => GetOwnedCompanions().Contains(companionId);

        public static void AddOwnedCompanion(string companionId)
        {
            if (string.IsNullOrEmpty(companionId)) return;
            var owned = GetOwnedCompanions();
            if (owned.Contains(companionId)) return;
            owned.Add(companionId);
            PlayerPrefs.SetString(OwnedKey, string.Join(",", owned));
            PlayerPrefs.Save();
        }

        public static string GetEquippedCompanion() => PlayerPrefs.GetString(EquippedKey, string.Empty);

        public static void SetEquippedCompanion(string companionId)
        {
            PlayerPrefs.SetString(EquippedKey, companionId ?? string.Empty);
            PlayerPrefs.Save();
        }
    }
}
