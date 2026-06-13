// Sparkle Cookie - custom code.
using System;

namespace SparkleCookie.SparkleMeter
{
    /// <summary>
    /// HEADLINE MECHANIC: the Sparkle Surge meter.
    ///
    /// Pure, Unity-independent logic (driven by <see cref="Tick"/>) so it can be
    /// unit-tested. Each animal match adds charge scaled by match size AND the
    /// current combo chain (back-to-back matches within <see cref="ComboWindow"/>
    /// compound). Idle time drains the charge. When charge fills, the animals throw
    /// a "Sparkle Party" (a board-wide burst) and the meter resets.
    /// </summary>
    public class SparkleMeter
    {
        public float Capacity = 100f;
        public float ComboWindow = 2.5f;    // seconds to keep a chain alive
        public float DrainPerSecond = 6f;   // passive drain once the chain lapses
        public float ComboBonusPerLink = 0.25f;

        public float Charge { get; private set; }
        public int ComboChain { get; private set; }
        public float Normalized => Capacity <= 0f ? 0f : Clamp01(Charge / Capacity);

        private float comboTimer;

        public event Action<float> ChargeChanged;   // normalized 0..1
        public event Action<int> ComboChanged;
        public event Action Burst;

        /// <summary>Feed a resolved animal match into the meter.</summary>
        public void AddMatch(int animalsCleared)
        {
            if (animalsCleared <= 0) return;

            ComboChain++;
            comboTimer = ComboWindow;
            ComboChanged?.Invoke(ComboChain);

            float gain = animalsCleared * (1f + ComboBonusPerLink * (ComboChain - 1));
            Charge += gain;

            if (Charge >= Capacity)
            {
                DoBurst();
            }
            else
            {
                ChargeChanged?.Invoke(Normalized);
            }
        }

        /// <summary>Advance time: lapse the combo window, then drain when idle.</summary>
        public void Tick(float deltaTime)
        {
            if (comboTimer > 0f)
            {
                comboTimer -= deltaTime;
                if (comboTimer <= 0f && ComboChain != 0)
                {
                    ComboChain = 0;
                    ComboChanged?.Invoke(0);
                }
            }
            else if (Charge > 0f)
            {
                Charge = Math.Max(0f, Charge - DrainPerSecond * deltaTime);
                ChargeChanged?.Invoke(Normalized);
            }
        }

        public void Reset()
        {
            Charge = 0f;
            ComboChain = 0;
            comboTimer = 0f;
            ChargeChanged?.Invoke(0f);
            ComboChanged?.Invoke(0);
        }

        private void DoBurst()
        {
            Charge = 0f;
            ComboChain = 0;
            comboTimer = 0f;
            ChargeChanged?.Invoke(0f);
            ComboChanged?.Invoke(0);
            Burst?.Invoke();
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
