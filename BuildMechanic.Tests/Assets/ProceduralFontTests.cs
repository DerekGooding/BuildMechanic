using BuildMechanic.UI;
using Xunit;

namespace BuildMechanic.Tests.Assets
{
    public class ProceduralFontTests
    {
        [Theory]
        [InlineData('\u2014', '-')] // Em-dash
        [InlineData('\u2013', '-')] // En-dash
        [InlineData('\u2018', '\'')] // Left single curly quote
        [InlineData('\u2019', '\'')] // Right single curly quote
        [InlineData('\u201C', '"')]  // Left double curly quote
        [InlineData('\u201D', '"')]  // Right double curly quote
        [InlineData('\u00A0', ' ')]  // Non-breaking space
        [InlineData('\u2026', '.')]  // Ellipsis
        [InlineData('A', 'A')]
        [InlineData('5', '5')]
        public void NormalizeChar_SanitizesUnicode_ToSafeAscii(char input, char expected)
        {
            char result = ProceduralFont.NormalizeChar(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("[1] SCANNER")]
        [InlineData("[2] DRIVER [FWD]")]
        [InlineData("[2] DRIVER [REV]")]
        [InlineData("[3] PRY / MOUNT")]
        [InlineData("[4] GRINDER")]
        [InlineData("[5] WELDER")]
        public void HotbarTitles_AtCurrentScale_FitWithinSlotBounds(string title)
        {
            // Each hotbar slot is 860px / 5 = 172px wide
            float slotWidth = 172f;
            float fontScale = 1.25f;

            float textWidth = ProceduralFont.CalculateStringWidth(title, fontScale);

            // Must fit comfortably with at least 15px total margin
            Assert.True(textWidth < slotWidth - 15f,
                $"Hotbar title '{title}' width ({textWidth}px) exceeds slot width ({slotWidth}px)!");
        }
    }
}
