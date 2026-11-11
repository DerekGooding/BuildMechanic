using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;

namespace BuildMechanic.Inspector
{
    public enum ModelCategory
    {
        ChassisAndCore,
        LimbsAndJoints,
        ArmorAndFasteners,
        GantryRig,
        ShopTools,
        SubsystemsAndWelds,
        HangarEnvironment
    }

    public enum InspectorRenderMode
    {
        ShadedLit,
        FlatUnlit,
        Normals,
        Wireframe
    }

    public class InspectableModel
    {
        public string Name { get; }
        public ModelCategory Category { get; }
        public string Description { get; }
        public BoundingBox Bounds { get; }
        public int VertexCount { get; }
        public int TriangleCount { get; }
        public Action<GraphicsDevice, BasicEffect, InspectorRenderMode> DrawAction { get; }

        public InspectableModel(
            string name,
            ModelCategory category,
            string description,
            BoundingBox bounds,
            int vertexCount,
            int triangleCount,
            Action<GraphicsDevice, BasicEffect, InspectorRenderMode> drawAction)
        {
            Name = name;
            Category = category;
            Description = description;
            Bounds = bounds;
            VertexCount = vertexCount;
            TriangleCount = triangleCount;
            DrawAction = drawAction ?? throw new ArgumentNullException(nameof(drawAction));
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, InspectorRenderMode mode)
        {
            DrawAction(device, effect, mode);
        }

        public static InspectableModel FromSimpleMesh(
            string name,
            ModelCategory category,
            string description,
            SimpleMesh mesh,
            Texture2D texture,
            Vector3 diffuseColor,
            BoundingBox bounds,
            Matrix? worldTransform = null)
        {
            Matrix baseWorld = worldTransform ?? Matrix.Identity;

            return new InspectableModel(
                name,
                category,
                description,
                bounds,
                mesh.VertexCount,
                mesh.PrimitiveCount,
                (device, effect, mode) =>
                {
                    effect.World = baseWorld;
                    if (mode == InspectorRenderMode.Normals)
                    {
                        effect.TextureEnabled = false;
                        effect.LightingEnabled = true;
                        effect.AmbientLightColor = new Vector3(0.15f, 0.15f, 0.15f);
                        effect.DirectionalLight0.Enabled = true;
                        effect.DirectionalLight0.Direction = new Vector3(-1, 0, 0);
                        effect.DirectionalLight0.DiffuseColor = new Vector3(1, 0.1f, 0.1f);
                        effect.DirectionalLight1.Enabled = true;
                        effect.DirectionalLight1.Direction = new Vector3(0, -1, 0);
                        effect.DirectionalLight1.DiffuseColor = new Vector3(0.1f, 1, 0.1f);
                        effect.DirectionalLight2.Enabled = true;
                        effect.DirectionalLight2.Direction = new Vector3(0, 0, -1);
                        effect.DirectionalLight2.DiffuseColor = new Vector3(0.1f, 0.2f, 1);
                        effect.DiffuseColor = Vector3.One;
                    }
                    else if (mode == InspectorRenderMode.FlatUnlit)
                    {
                        effect.LightingEnabled = false;
                        effect.TextureEnabled = texture != null;
                        if (texture != null) effect.Texture = texture;
                        effect.DiffuseColor = diffuseColor;
                    }
                    else
                    {
                        effect.TextureEnabled = texture != null;
                        if (texture != null) effect.Texture = texture;
                        effect.DiffuseColor = diffuseColor;
                    }

                    mesh.Draw(device, effect);
                });
        }
    }
}
