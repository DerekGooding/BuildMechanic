using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MeshGeometryHelper
    {
        public static void AddQuad(
            List<VertexPositionNormalTexture> vertices,
            List<ushort> indices,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Vector3 normal,
            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            ushort s = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
            vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
            vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));
            vertices.Add(new VertexPositionNormalTexture(p3, normal, uv3));

            Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
            if (Vector3.Dot(cross, normal) >= 0f)
            {
                indices.Add(s); indices.Add((ushort)(s + 1)); indices.Add((ushort)(s + 2));
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 3));
            }
            else
            {
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
                indices.Add(s); indices.Add((ushort)(s + 3)); indices.Add((ushort)(s + 2));
            }
        }

        public static void AddQuad(
            List<VertexPositionNormalTexture> vertices,
            List<ushort> indices,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Vector3 normal)
        {
            AddQuad(
                vertices, indices, p0, p1, p2, p3, normal,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
        }

        public static void AddTri(
            List<VertexPositionNormalTexture> vertices,
            List<ushort> indices,
            Vector3 p0, Vector3 p1, Vector3 p2,
            Vector3 normal,
            Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            ushort s = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
            vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
            vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));

            Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
            if (Vector3.Dot(cross, normal) >= 0f)
            {
                indices.Add(s); indices.Add((ushort)(s + 1)); indices.Add((ushort)(s + 2));
            }
            else
            {
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
            }
        }

        public static void AddCylinderWall(List<ushort> indices, ushort b)
        {
            indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
            indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
        }

        public static void AddTopCapTri(List<ushort> indices, ushort center, ushort i)
        {
            indices.Add(center); indices.Add((ushort)(center + 2 + i)); indices.Add((ushort)(center + 1 + i));
        }

        public static void AddBottomCapTri(List<ushort> indices, ushort center, ushort i)
        {
            indices.Add(center); indices.Add((ushort)(center + 1 + i)); indices.Add((ushort)(center + 2 + i));
        }
    }
}
