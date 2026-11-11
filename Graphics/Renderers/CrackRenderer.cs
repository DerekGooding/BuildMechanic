using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics.Renderers
{
    public class CrackRenderer : IDisposable
    {
        private readonly SimpleMesh _boxMesh;
        private readonly SimpleMesh _weldMesh;
        private readonly Texture2D _pixelTex;

        public CrackRenderer(GraphicsDevice device, SimpleMesh boxMesh, Texture2D pixelTex)
        {
            _boxMesh = boxMesh;
            _pixelTex = pixelTex;
            _weldMesh = CrackMeshBuilder.CreateWeldBead(device, 1.0f, 0.036f, 0.018f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, ToolManager tools)
        {
            effect.Texture = _pixelTex;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();

                foreach (var crack in plate.Cracks)
                {
                    int segCount = crack.LocalPoints.Count - 1;
                    if (segCount <= 0) continue;

                    for (int i = 0; i < segCount; i++)
                    {
                        Vector3 p0 = Vector3.Transform(crack.LocalPoints[i], plateWorld);
                        Vector3 p1 = Vector3.Transform(crack.LocalPoints[i + 1], plateWorld);
                        Vector3 mid = (p0 + p1) * 0.5f;
                        Vector3 dir = p1 - p0;
                        float len = dir.Length();
                        if (len < 0.001f) continue;

                        dir /= len;
                        float segStart = (float)i / segCount;
                        float segEnd = (float)(i + 1) / segCount;
                        float progress = crack.StageProgress;

                        Color segColor = crack.Stage switch
                        {
                            CrackStage.Fractured => (progress >= segEnd) ? new Color(210, 218, 230) :
                                ((progress > segStart && tools.IsOperating && tools.HoveredCrack == crack) ? new Color(255, 240, 150) : new Color(24, 25, 28)),
                            CrackStage.PreppedGroove => (progress >= segEnd) ? new Color(255, 130, 20) :
                                ((progress > segStart && tools.IsOperating && tools.HoveredCrack == crack) ? new Color(255, 255, 220) : new Color(210, 218, 230)),
                            CrackStage.WeldedBead => (progress >= segEnd) ? new Color(185, 195, 210) :
                                ((progress > segStart && tools.IsOperating && tools.HoveredCrack == crack) ? new Color(255, 225, 110) : new Color(245, 120, 20)),
                            _ => new Color(185, 195, 205)
                        };

                        float thickness = crack.Stage == CrackStage.WeldedBead ? 0.038f : 0.024f;
                        if (tools.HoveredCrack == crack && !tools.IsOperating) segColor = Color.Yellow;

                        effect.DiffuseColor = segColor.ToVector3();
                        effect.World = Matrix.CreateScale(thickness, thickness, len) * Matrix.CreateWorld(mid, dir, Vector3.Up);

                        if (crack.Stage == CrackStage.WeldedBead && progress >= segEnd)
                        {
                            _weldMesh.Draw(device, effect);
                        }
                        else
                        {
                            _boxMesh.Draw(device, effect);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _weldMesh?.Dispose();
        }
    }
}
