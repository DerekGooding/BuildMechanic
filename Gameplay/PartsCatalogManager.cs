using System.Collections.Generic;
using BuildMechanic.Simulation.Graph;

namespace BuildMechanic.Gameplay
{
    public class PartsCatalogManager
    {
        public static PartsCatalogManager Instance { get; private set; }

        public Dictionary<ComponentCategory, int> Stock { get; } = new Dictionary<ComponentCategory, int>();
        public int TotalScrapped { get; private set; }

        public static readonly (ComponentCategory Category, string Name, int Cost)[] CatalogItems =
        {
            (ComponentCategory.ImpellerPump, "Coolant Impeller & Viton Seal", 350),
            (ComponentCategory.PowerPCB, "Power Distribution PCB", 420),
            (ComponentCategory.Bearing, "Heavy Torque Bearing Assembly", 380),
            (ComponentCategory.Accumulator, "Hydraulic Pressure Accumulator", 320)
        };

        public PartsCatalogManager()
        {
            Instance = this;
            ResetStock();
        }

        public void ResetStock()
        {
            Stock[ComponentCategory.ImpellerPump] = 1;
            Stock[ComponentCategory.PowerPCB] = 1;
            Stock[ComponentCategory.Bearing] = 1;
            Stock[ComponentCategory.Accumulator] = 1;
            TotalScrapped = 0;
        }

        public int GetStock(ComponentCategory cat) => Stock.TryGetValue(cat, out int count) ? count : 0;

        public bool OrderPart(ComponentCategory cat, int cost)
        {
            if (UpgradeManager.Instance != null)
            {
                if (UpgradeManager.Instance.Credits < cost) return false;
                UpgradeManager.Instance.Credits -= cost;
            }
            Stock[cat] = GetStock(cat) + 1;
            return true;
        }

        public bool TryConsumeForReplacement(ComponentCategory cat, int fallbackCost)
        {
            int cur = GetStock(cat);
            if (cur > 0)
            {
                Stock[cat] = cur - 1;
                return true;
            }
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.Credits >= fallbackCost)
            {
                UpgradeManager.Instance.Credits -= fallbackCost;
                return true;
            }
            return false;
        }

        public void ScrapPart(int salvageValue)
        {
            TotalScrapped++;
            UpgradeManager.Instance?.AddReward(salvageValue);
        }
    }
}
