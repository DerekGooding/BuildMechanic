using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.UI;

namespace BuildMechanic.Inspector
{
    public class InspectorUI
    {
        private readonly Texture2D _pixel;
        private readonly ProceduralFont _font;
        private MouseState _prevMouse;

        public InspectorUI(GraphicsDevice device, Texture2D pixel)
        {
            _pixel = pixel;
            _font = new ProceduralFont(device);
        }

        public void Draw(
            SpriteBatch sb,
            Viewport vp,
            ModelCatalog catalog,
            InspectorCamera camera,
            InspectorRenderMode mode,
            bool wireframe,
            bool grid)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            int sw = vp.Width;
            int sh = vp.Height;

            DrawSidebar(sb, catalog, sh);
            DrawTopBar(sb, catalog, sw, mode, wireframe, grid, camera.IsSpinning);
            DrawBottomBar(sb, sw, sh);

            sb.End();
        }

        private void DrawSidebar(SpriteBatch sb, ModelCatalog catalog, int sh)
        {
            int sideW = 270;
            // Background
            sb.Draw(_pixel, new Rectangle(0, 0, sideW, sh), new Color(16, 20, 26, 240));
            sb.Draw(_pixel, new Rectangle(sideW - 2, 0, 2, sh), new Color(40, 60, 80));

            // Header
            sb.Draw(_pixel, new Rectangle(0, 0, sideW, 46), new Color(24, 30, 40));
            sb.Draw(_pixel, new Rectangle(0, 44, sideW, 2), new Color(0, 200, 240));
            _font.DrawString(sb, "MODEL CATALOG", new Vector2(16, 14), Color.Cyan, 1.8f);

            // Category selector tabs
            int y = 56;
            _font.DrawString(sb, "CATEGORIES", new Vector2(16, y), Color.DarkGray, 1.2f);
            y += 18;

            foreach (var cat in catalog.Categories)
            {
                bool isCatActive = cat == catalog.ActiveCategory;
                string catName = FormatCategoryName(cat);
                Color tabBg = isCatActive ? new Color(0, 140, 180, 180) : new Color(25, 32, 42, 160);
                Color tabText = isCatActive ? Color.White : Color.Silver;

                sb.Draw(_pixel, new Rectangle(12, y, sideW - 24, 22), tabBg);
                if (isCatActive)
                    sb.Draw(_pixel, new Rectangle(12, y, 4, 22), Color.Gold);

                _font.DrawString(sb, catName, new Vector2(22, y + 4), tabText, 1.25f);
                y += 26;
            }

            // Model List in Active Category
            y += 10;
            _font.DrawString(sb, "MODELS IN CATEGORY", new Vector2(16, y), Color.DarkGray, 1.2f);
            y += 18;

            var models = catalog.ActiveCategoryModels;
            for (int i = 0; i < models.Count && y < sh - 40; i++)
            {
                bool isModelActive = i == catalog.ActiveModelIndex;
                Color itemBg = isModelActive ? new Color(45, 75, 110, 220) : new Color(20, 26, 34, 120);
                Color itemText = isModelActive ? Color.Cyan : Color.LightGray;

                sb.Draw(_pixel, new Rectangle(12, y, sideW - 24, 24), itemBg);
                if (isModelActive)
                    sb.Draw(_pixel, new Rectangle(12, y, 3, 24), Color.Cyan);

                string truncated = Truncate(models[i].Name, 26);
                _font.DrawString(sb, $"{i + 1}. {truncated}", new Vector2(20, y + 5), itemText, 1.25f);
                y += 28;
            }
        }

        private void DrawTopBar(
            SpriteBatch sb,
            ModelCatalog catalog,
            int sw,
            InspectorRenderMode mode,
            bool wireframe,
            bool grid,
            bool spinning)
        {
            var model = catalog.ActiveModel;
            int sideW = 270;
            int topH = 68;

            // Header background
            sb.Draw(_pixel, new Rectangle(sideW, 0, sw - sideW, topH), new Color(14, 18, 24, 235));
            sb.Draw(_pixel, new Rectangle(sideW, topH - 2, sw - sideW, 2), new Color(35, 50, 70));

            if (model != null)
            {
                // Title & Category
                _font.DrawString(sb, model.Name.ToUpper(), new Vector2(sideW + 20, 10), Color.White, 1.9f);
                _font.DrawString(sb, model.Description, new Vector2(sideW + 20, 32), Color.Silver, 1.2f);

                // Telemetry Card
                Vector3 dims = model.Bounds.Max - model.Bounds.Min;
                string stats = $"VERTS: {model.VertexCount:N0}  |  TRIS: {model.TriangleCount:N0}  |  SIZE: {dims.X:F2}m x {dims.Y:F2}m x {dims.Z:F2}m";
                _font.DrawString(sb, stats, new Vector2(sideW + 20, 48), Color.Gold, 1.2f);
            }

            // Mode Pills (top-right)
            int pillX = sw - 480;
            int pillY = 14;

            DrawPill(sb, ref pillX, pillY, "LIT [L]", mode == InspectorRenderMode.ShadedLit);
            DrawPill(sb, ref pillX, pillY, "UNLIT [U]", mode == InspectorRenderMode.FlatUnlit);
            DrawPill(sb, ref pillX, pillY, "NORMALS [N]", mode == InspectorRenderMode.Normals);
            DrawPill(sb, ref pillX, pillY, "WIREFRAME [W]", wireframe);
            DrawPill(sb, ref pillX, pillY, "GRID [G]", grid);
            DrawPill(sb, ref pillX, pillY, "SPIN [SPACE]", spinning);
        }

        private void DrawPill(SpriteBatch sb, ref int x, int y, string label, bool active)
        {
            int w = 72;
            int h = 24;
            Color bg = active ? new Color(0, 180, 220) : new Color(30, 40, 52);
            Color text = active ? Color.Black : Color.Silver;

            sb.Draw(_pixel, new Rectangle(x, y, w, h), bg);
            _font.DrawString(sb, label, new Vector2(x + 5, y + 6), text, 1.1f);
            x += w + 6;
        }

        private void DrawBottomBar(SpriteBatch sb, int sw, int sh)
        {
            int sideW = 270;
            int botH = 28;
            sb.Draw(_pixel, new Rectangle(sideW, sh - botH, sw - sideW, botH), new Color(14, 18, 24, 235));
            sb.Draw(_pixel, new Rectangle(sideW, sh - botH, sw - sideW, 1), new Color(35, 50, 70));

            string hints = "[ / ]: Next/Prev Model  |  UP/DOWN: Category  |  1-4: Preset Angles  |  F: Focus Bounds  |  ESC: Exit";
            _font.DrawString(sb, hints, new Vector2(sideW + 20, sh - botH + 7), new Color(140, 160, 180), 1.2f);
        }

        public void HandleInput(
            ModelCatalog catalog,
            InspectorCamera camera,
            ref InspectorRenderMode mode,
            ref bool wireframe,
            ref bool grid)
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
            {
                int mx = mouse.X;
                int my = mouse.Y;

                // Check Category click
                if (mx >= 12 && mx <= 258 && my >= 74 && my <= 74 + catalog.Categories.Count * 26)
                {
                    int catIdx = (my - 74) / 26;
                    if (catIdx >= 0 && catIdx < catalog.Categories.Count)
                    {
                        catalog.SelectCategory(catalog.Categories[catIdx]);
                        if (catalog.ActiveModel != null)
                            camera.FocusOnBounds(catalog.ActiveModel.Bounds);
                    }
                }

                // Check Model item click
                int modelStartY = 74 + catalog.Categories.Count * 26 + 28;
                if (mx >= 12 && mx <= 258 && my >= modelStartY)
                {
                    int mIdx = (my - modelStartY) / 28;
                    if (mIdx >= 0 && mIdx < catalog.ActiveCategoryModels.Count)
                    {
                        catalog.SelectModel(mIdx);
                        if (catalog.ActiveModel != null)
                            camera.FocusOnBounds(catalog.ActiveModel.Bounds);
                    }
                }
            }
            _prevMouse = mouse;
        }

        private static string FormatCategoryName(ModelCategory cat) => cat switch
        {
            ModelCategory.ChassisAndCore => "Chassis & Core",
            ModelCategory.LimbsAndJoints => "Limbs & Joints",
            ModelCategory.ArmorAndFasteners => "Armor & Fasteners",
            ModelCategory.GantryRig => "Gantry Rig",
            ModelCategory.ShopTools => "Shop Tools",
            ModelCategory.SubsystemsAndWelds => "Subsystems & Welds",
            ModelCategory.HangarEnvironment => "Hangar Environment",
            _ => cat.ToString()
        };

        private static string Truncate(string val, int maxLen) =>
            val.Length <= maxLen ? val : val.Substring(0, maxLen - 2) + "..";
    }
}
