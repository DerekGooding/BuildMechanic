using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation.Archetypes
{
    public enum MechClass
    {
        VanguardTitan,
        SpectreRecon,
        ColossusSiege,
        ArachnidStrider
    }

    public class MechArchetype
    {
        public MechClass Class { get; init; }
        public string Name { get; init; }
        public string Role { get; init; }
        public Color PrimaryArmorColor { get; init; }
        public Vector3 ChassisAccentTint { get; init; }
        public float PayoutMultiplier { get; init; }

        public static MechArchetype GetForContract(int contractNumber) => ((contractNumber - 1) % 4) switch
        {
            0 => new MechArchetype
            {
                Class = MechClass.VanguardTitan,
                Name = "Vanguard Titan",
                Role = "Medium Brawler",
                PrimaryArmorColor = new Color(95, 110, 88),
                ChassisAccentTint = new Vector3(0.65f, 0.68f, 0.74f),
                PayoutMultiplier = 1.0f
            },
            1 => new MechArchetype
            {
                Class = MechClass.SpectreRecon,
                Name = "Spectre Recon",
                Role = "Light Scout",
                PrimaryArmorColor = new Color(68, 92, 118),
                ChassisAccentTint = new Vector3(0.55f, 0.72f, 0.85f),
                PayoutMultiplier = 1.15f
            },
            2 => new MechArchetype
            {
                Class = MechClass.ColossusSiege,
                Name = "Colossus Siege",
                Role = "Heavy Assault",
                PrimaryArmorColor = new Color(110, 82, 68),
                ChassisAccentTint = new Vector3(0.75f, 0.62f, 0.56f),
                PayoutMultiplier = 1.35f
            },
            _ => new MechArchetype
            {
                Class = MechClass.ArachnidStrider,
                Name = "Arachnid Strider",
                Role = "Quad-Artillery",
                PrimaryArmorColor = new Color(128, 108, 68),
                ChassisAccentTint = new Vector3(0.78f, 0.72f, 0.52f),
                PayoutMultiplier = 1.50f
            }
        };
    }
}
