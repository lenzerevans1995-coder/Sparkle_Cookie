// Sparkle Cookie - custom code.
namespace SparkleCookie.Config
{
    /// <summary>
    /// Single source of truth that maps the engine's six generic block colors
    /// (Block1..Block6 -> indices 0..5) to Sparkle Cookie's animal match-pieces.
    /// UI, goals, and companion logic read display names/colors from here so a
    /// re-pick is a one-line change. The <c>AnimalSprite</c> ids reference the
    /// flattened GUI-pack animal PNGs (PSD Sources/Icons/Game/Animals/PNG) and are
    /// documented in docs/ART_RECIPES.md (the actual PNGs are licensed, copied into
    /// Assets/SparkleCookie/Art locally, and not committed to git).
    /// </summary>
    public static class SparkleTheme
    {
        public struct AnimalDef
        {
            public string DisplayName;
            public string AnimalSprite;  // GUI-pack flattened animal PNG name
            public string TintHex;       // sparkle/accent color
        }

        /// <summary>Indexed by engine block index (Block1 == [0]).</summary>
        public static readonly AnimalDef[] Animals =
        {
            new AnimalDef { DisplayName = "Bunny",   AnimalSprite = "Bunny - Pink", TintHex = "#F2A0B5" },
            new AnimalDef { DisplayName = "Cat",     AnimalSprite = "Cat - Orange", TintHex = "#F2A65A" },
            new AnimalDef { DisplayName = "Panda",   AnimalSprite = "Panda",        TintHex = "#9AA7B0" },
            new AnimalDef { DisplayName = "Pig",     AnimalSprite = "Pig",          TintHex = "#F4B6C2" },
            new AnimalDef { DisplayName = "Frog",    AnimalSprite = "Frog",         TintHex = "#8BD45A" },
            new AnimalDef { DisplayName = "Owl",     AnimalSprite = "Owl",          TintHex = "#B98A5E" },
        };

        public static string DisplayName(int blockIndex) =>
            blockIndex >= 0 && blockIndex < Animals.Length ? Animals[blockIndex].DisplayName : "Animal";

        public const string GameTitle = "Sparkle Cookie";
    }
}
