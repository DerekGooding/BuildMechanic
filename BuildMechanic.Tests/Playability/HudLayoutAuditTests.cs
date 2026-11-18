using BuildMechanic.UI;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Playability
{
    public class HudLayoutAuditTests
    {
        [Theory]
        [InlineData(1024, 768)]
        [InlineData(1280, 720)]
        [InlineData(1600, 900)]
        [InlineData(1920, 1080)]
        public void AllHudPanels_AreInsideViewport_AndNeverOverlap(int sw, int sh)
        {
            Rectangle viewport = new Rectangle(0, 0, sw, sh);

            Rectangle catalogBtn = HUD.GetCatalogButtonRect(sw);
            Rectangle shopBtn = HUD.GetShopButtonRect(sw);
            Rectangle testBtn = HUD.GetTestButtonRect(sw);
            Rectangle helpBtn = HUD.GetHelpButtonRect(sw);

            Rectangle sectorBar = HUDSectorBar.GetBounds();
            Rectangle contractTracker = HUD.GetContractTrackerBounds(sw, objectiveCount: 9);
            Rectangle targetCard = HUD.GetTargetCardBounds(sw);
            Rectangle hotbar = HUDHotbar.GetBounds(sw, sh);
            Rectangle helpDrawer = HUDSectorBar.GetHelpDrawerBounds(sw, sh);

            Rectangle[] allRects =
            {
                catalogBtn, shopBtn, testBtn, helpBtn,
                sectorBar, contractTracker, targetCard, hotbar, helpDrawer
            };

            foreach (var r in allRects)
            {
                Assert.True(viewport.Contains(r), $"HUD rect {r} extends outside {sw}x{sh} viewport.");
            }

            // Top-bar buttons do not overlap each other
            Assert.False(catalogBtn.Intersects(shopBtn));
            Assert.False(shopBtn.Intersects(testBtn));
            Assert.False(testBtn.Intersects(helpBtn));

            // Left column panels do not overlap each other or right column or hotbar
            Assert.False(sectorBar.Intersects(contractTracker), "SectorBar overlaps ContractTracker.");
            Assert.False(sectorBar.Intersects(targetCard), "SectorBar overlaps TargetCard.");
            Assert.False(contractTracker.Intersects(targetCard), "ContractTracker overlaps TargetCard.");
            Assert.False(contractTracker.Intersects(hotbar), "ContractTracker overlaps Hotbar.");

            // Right column panels do not overlap each other or hotbar
            Assert.False(targetCard.Intersects(helpDrawer), "TargetCard overlaps HelpDrawer.");
            Assert.False(helpDrawer.Intersects(hotbar), "HelpDrawer overlaps Hotbar.");
            Assert.False(targetCard.Intersects(hotbar), "TargetCard overlaps Hotbar.");
        }
    }
}
