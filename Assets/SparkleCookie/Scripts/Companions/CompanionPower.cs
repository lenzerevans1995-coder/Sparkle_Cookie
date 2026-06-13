// Sparkle Cookie - custom code.
using System.Collections.Generic;
using SparkleCookie.SparkleMeter;

namespace SparkleCookie.Companions
{
    /// <summary>
    /// A power granted by the equipped animal companion. STUB LAYER: wired to the
    /// gameplay seam (the <see cref="SparkleMeter.SparkleMeter"/> exposed by the
    /// board) but disabled by default behind <see cref="CompanionPowers.Enabled"/>
    /// until the collection phase ships.
    /// </summary>
    public interface ICompanionPower
    {
        string Id { get; }
        void OnLevelStart(SparkleMeter.SparkleMeter meter);
        void OnTeardown(SparkleMeter.SparkleMeter meter);
    }

    /// <summary>Panda: pre-charges the Sparkle meter at level start.</summary>
    public class PandaPrechargePower : ICompanionPower
    {
        public string Id => "panda_precharge";
        public void OnLevelStart(SparkleMeter.SparkleMeter meter) { if (meter != null) meter.AddMatch(12); }
        public void OnTeardown(SparkleMeter.SparkleMeter meter) { }
    }

    /// <summary>Cat: longer combo window so sparkle chains are easier to keep alive.</summary>
    public class CatChainPower : ICompanionPower
    {
        public string Id => "cat_chain";
        public void OnLevelStart(SparkleMeter.SparkleMeter meter) { if (meter != null) meter.ComboWindow += 1.0f; }
        public void OnTeardown(SparkleMeter.SparkleMeter meter) { if (meter != null) meter.ComboWindow -= 1.0f; }
    }

    /// <summary>Lookup + global on/off flag for the (stubbed) power system.</summary>
    public static class CompanionPowers
    {
        public static bool Enabled = false; // flip on during the collection phase

        private static readonly Dictionary<string, ICompanionPower> Registry =
            new Dictionary<string, ICompanionPower>
            {
                { "panda_precharge", new PandaPrechargePower() },
                { "cat_chain", new CatChainPower() },
            };

        public static ICompanionPower Resolve(string powerId)
        {
            if (string.IsNullOrEmpty(powerId)) return null;
            return Registry.TryGetValue(powerId, out var p) ? p : null;
        }
    }
}
