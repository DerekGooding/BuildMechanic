using System;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Graphics
{
    public enum DecalType
    {
        None,
        HazardChevrons,
        TitanStar,
        WarningHighVoltage
    }

    public static class ArmorDecal
    {
        public static readonly Color[] PaintPalette = new Color[]
        {
            Color.White,                        // 0: Factory Steel
            new Color(95, 110, 85),              // 1: Military Olive Drab
            new Color(230, 175, 45),             // 2: Industrial Hazard Yellow
            new Color(45, 50, 58),               // 3: Stealth Matte Carbon
            new Color(185, 90, 40)               // 4: Rust-Guard Desert Tan
        };

        public static readonly string[] PaletteNames = new string[]
        {
            "FACTORY STEEL",
            "MILITARY OLIVE",
            "HAZARD YELLOW",
            "STEALTH CARBON",
            "DESERT TAN"
        };
    }
}
