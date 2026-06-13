// Sparkle Cookie - custom code.
using System.Collections.Generic;
using UnityEngine;

namespace SparkleCookie.Companions
{
    /// <summary>
    /// Registry of every collectible animal companion. Create one asset via
    /// Assets > Create > SparkleCookie > Companion Database and assign it where
    /// the collection UI / power system needs it.
    /// </summary>
    [CreateAssetMenu(fileName = "CompanionDatabase", menuName = "SparkleCookie/Companion Database")]
    public class CompanionDatabase : ScriptableObject
    {
        public List<AnimalCompanion> companions = new List<AnimalCompanion>();

        public AnimalCompanion Get(string id) => companions.Find(c => c != null && c.id == id);
    }
}
