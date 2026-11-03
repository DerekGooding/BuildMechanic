using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.UI
{
    public class ProceduralFont
    {
        private readonly Texture2D _fontTexture;
        private const int CharWidth = 6;
        private const int CharHeight = 8;
        private readonly Dictionary<char, byte[]> _glyphs = new Dictionary<char, byte[]>();

        public ProceduralFont(GraphicsDevice device)
        {
            InitGlyphs();

            // Create a texture containing all ASCII chars 32..126
            int cols = 16;
            int rows = 6;
            int texWidth = cols * CharWidth;
            int texHeight = rows * CharHeight;

            _fontTexture = new Texture2D(device, texWidth, texHeight);
            var colorData = new Color[texWidth * texHeight];

            for (int i = 32; i < 127; i++)
            {
                char c = (char)i;
                int glyphIndex = i - 32;
                int cellX = (glyphIndex % cols) * CharWidth;
                int cellY = (glyphIndex / cols) * CharHeight;

                byte[] rowsData = _glyphs.TryGetValue(c, out var data) ? data : _glyphs['?'];

                for (int y = 0; y < 7; y++)
                {
                    byte row = (y < rowsData.Length) ? rowsData[y] : (byte)0;
                    for (int x = 0; x < 5; x++)
                    {
                        bool bit = ((row >> (4 - x)) & 1) == 1;
                        int px = cellX + x;
                        int py = cellY + y;
                        colorData[py * texWidth + px] = bit ? Color.White : Color.Transparent;
                    }
                }
            }

            _fontTexture.SetData(colorData);
        }

        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 2.0f)
        {
            if (string.IsNullOrEmpty(text) || _fontTexture == null) return;

            int cols = 16;
            float curX = position.X;
            float curY = position.Y;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    curX = position.X;
                    curY += (CharHeight + 1) * scale;
                    continue;
                }

                int ascii = (int)c;
                if (ascii < 32 || ascii > 126) ascii = (int)'?';

                int glyphIndex = ascii - 32;
                int cellX = (glyphIndex % cols) * CharWidth;
                int cellY = (glyphIndex / cols) * CharHeight;

                Rectangle src = new Rectangle(cellX, cellY, CharWidth, CharHeight);
                Rectangle dst = new Rectangle((int)curX, (int)curY, (int)(CharWidth * scale), (int)(CharHeight * scale));

                spriteBatch.Draw(_fontTexture, dst, src, color);
                curX += (CharWidth + 1) * scale;
            }
        }

        public Vector2 MeasureString(string text, float scale = 2.0f)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;
            float maxWidth = 0f;
            float curWidth = 0f;
            float lines = 1;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    maxWidth = Math.Max(maxWidth, curWidth);
                    curWidth = 0f;
                    lines++;
                }
                else
                {
                    curWidth += (CharWidth + 1) * scale;
                }
            }
            maxWidth = Math.Max(maxWidth, curWidth);
            return new Vector2(maxWidth, lines * (CharHeight + 1) * scale);
        }

        private void InitGlyphs()
        {
            // 5x7 bitmap font definitions (5 bits per row, 7 rows)
            _glyphs[' '] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _glyphs['!'] = new byte[] { 0x04, 0x04, 0x04, 0x04, 0x00, 0x04, 0x00 };
            _glyphs['"'] = new byte[] { 0x0A, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _glyphs['#'] = new byte[] { 0x0A, 0x1F, 0x0A, 0x0A, 0x1F, 0x0A, 0x00 };
            _glyphs['$'] = new byte[] { 0x04, 0x0F, 0x14, 0x0E, 0x05, 0x1E, 0x04 };
            _glyphs['%'] = new byte[] { 0x19, 0x19, 0x02, 0x04, 0x08, 0x13, 0x13 };
            _glyphs['&'] = new byte[] { 0x0C, 0x12, 0x14, 0x08, 0x15, 0x12, 0x0D };
            _glyphs['\''] = new byte[] { 0x04, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _glyphs['('] = new byte[] { 0x02, 0x04, 0x08, 0x08, 0x08, 0x04, 0x02 };
            _glyphs[')'] = new byte[] { 0x08, 0x04, 0x02, 0x02, 0x02, 0x04, 0x08 };
            _glyphs['*'] = new byte[] { 0x00, 0x04, 0x15, 0x0E, 0x15, 0x04, 0x00 };
            _glyphs['+'] = new byte[] { 0x00, 0x04, 0x04, 0x1F, 0x04, 0x04, 0x00 };
            _glyphs[','] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08 };
            _glyphs['-'] = new byte[] { 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00 };
            _glyphs['.'] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x06 };
            _glyphs['/'] = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x00, 0x00 };

            _glyphs['0'] = new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x11, 0x0E };
            _glyphs['1'] = new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x04, 0x0E };
            _glyphs['2'] = new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x08, 0x1F };
            _glyphs['3'] = new byte[] { 0x1F, 0x02, 0x04, 0x02, 0x01, 0x11, 0x0E };
            _glyphs['4'] = new byte[] { 0x02, 0x06, 0x0A, 0x12, 0x1F, 0x02, 0x02 };
            _glyphs['5'] = new byte[] { 0x1F, 0x10, 0x1E, 0x01, 0x01, 0x11, 0x0E };
            _glyphs['6'] = new byte[] { 0x06, 0x08, 0x10, 0x1E, 0x11, 0x11, 0x0E };
            _glyphs['7'] = new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x08, 0x08 };
            _glyphs['8'] = new byte[] { 0x0E, 0x11, 0x11, 0x0E, 0x11, 0x11, 0x0E };
            _glyphs['9'] = new byte[] { 0x0E, 0x11, 0x11, 0x0F, 0x01, 0x02, 0x0C };

            _glyphs[':'] = new byte[] { 0x00, 0x06, 0x06, 0x00, 0x06, 0x06, 0x00 };
            _glyphs[';'] = new byte[] { 0x00, 0x06, 0x06, 0x00, 0x06, 0x04, 0x08 };
            _glyphs['<'] = new byte[] { 0x02, 0x04, 0x08, 0x10, 0x08, 0x04, 0x02 };
            _glyphs['='] = new byte[] { 0x00, 0x1F, 0x00, 0x1F, 0x00, 0x00, 0x00 };
            _glyphs['>'] = new byte[] { 0x08, 0x04, 0x02, 0x01, 0x02, 0x04, 0x08 };
            _glyphs['?'] = new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x00, 0x04 };
            _glyphs['@'] = new byte[] { 0x0E, 0x11, 0x01, 0x0D, 0x15, 0x15, 0x0E };

            _glyphs['A'] = new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 };
            _glyphs['B'] = new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x11, 0x11, 0x1E };
            _glyphs['C'] = new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0E };
            _glyphs['D'] = new byte[] { 0x1C, 0x12, 0x11, 0x11, 0x11, 0x12, 0x1C };
            _glyphs['E'] = new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x1F };
            _glyphs['F'] = new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x10 };
            _glyphs['G'] = new byte[] { 0x0E, 0x11, 0x10, 0x17, 0x11, 0x11, 0x0E };
            _glyphs['H'] = new byte[] { 0x11, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 };
            _glyphs['I'] = new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0E };
            _glyphs['J'] = new byte[] { 0x07, 0x02, 0x02, 0x02, 0x02, 0x12, 0x0C };
            _glyphs['K'] = new byte[] { 0x11, 0x12, 0x14, 0x18, 0x14, 0x12, 0x11 };
            _glyphs['L'] = new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F };
            _glyphs['M'] = new byte[] { 0x11, 0x1B, 0x15, 0x15, 0x11, 0x11, 0x11 };
            _glyphs['N'] = new byte[] { 0x11, 0x11, 0x19, 0x15, 0x13, 0x11, 0x11 };
            _glyphs['O'] = new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E };
            _glyphs['P'] = new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x10 };
            _glyphs['Q'] = new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x15, 0x12, 0x0D };
            _glyphs['R'] = new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x14, 0x12, 0x11 };
            _glyphs['S'] = new byte[] { 0x0E, 0x11, 0x10, 0x0E, 0x01, 0x11, 0x0E };
            _glyphs['T'] = new byte[] { 0x1F, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 };
            _glyphs['U'] = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E };
            _glyphs['V'] = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x0A, 0x04 };
            _glyphs['W'] = new byte[] { 0x11, 0x11, 0x11, 0x15, 0x15, 0x1B, 0x11 };
            _glyphs['X'] = new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x0A, 0x11, 0x11 };
            _glyphs['Y'] = new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x04, 0x04, 0x04 };
            _glyphs['Z'] = new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x10, 0x1F };

            _glyphs['['] = new byte[] { 0x0E, 0x08, 0x08, 0x08, 0x08, 0x08, 0x0E };
            _glyphs['\\'] = new byte[] { 0x10, 0x08, 0x04, 0x02, 0x01, 0x00, 0x00 };
            _glyphs[']'] = new byte[] { 0x0E, 0x02, 0x02, 0x02, 0x02, 0x02, 0x0E };
            _glyphs['^'] = new byte[] { 0x04, 0x0A, 0x11, 0x00, 0x00, 0x00, 0x00 };
            _glyphs['_'] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F };

            // Lowercase mirrors uppercase or lowercase bitmap
            for (char c = 'a'; c <= 'z'; c++)
            {
                char upper = char.ToUpper(c);
                if (_glyphs.TryGetValue(upper, out var g))
                    _glyphs[c] = g;
            }
        }
    }
}
