using System.Collections.Generic;
using UnityEngine;

public static class ButterflyMeshBuilder
{
    private static readonly Dictionary<Sprite, Mesh> _cache = new();

    public static Mesh Build(Sprite sprite)
    {
        if (_cache.TryGetValue(sprite, out Mesh cached))
            return cached;

        Mesh mesh = BuildInternal(sprite);
        _cache[sprite] = mesh;
        return mesh;
    }

    private static Mesh BuildInternal(Sprite sprite)
    {
        Rect rect = sprite.rect;
        float aspect = rect.width / rect.height;
        float halfHeight = 0.5f;
        float halfWidth = halfHeight * aspect;

        float qw = halfWidth * 2f / 3f;
        float hh = halfHeight;

        Vector3[] verts = new Vector3[8];
        Vector2[] uvs = new Vector2[8];
        int[] tris = new int[18];

        float x0 = -halfWidth;
        float x1 = -halfWidth + qw;
        float x2 = -halfWidth + qw * 2f;
        float x3 = halfWidth;
        float z0 = -hh;
        float z1 = hh;

        Texture2D tex = sprite.texture;
        Rect texRect = sprite.textureRect;
        float uMin = texRect.xMin / tex.width;
        float uMax = texRect.xMax / tex.width;
        float vMin = texRect.yMin / tex.height;
        float vMax = texRect.yMax / tex.height;

        float u0 = uMin;
        float u1 = Mathf.Lerp(uMin, uMax, 1f / 3f);
        float u2 = Mathf.Lerp(uMin, uMax, 2f / 3f);
        float u3 = uMax;

        verts[0] = new Vector3(x0, 0, z0); uvs[0] = new Vector2(u0, vMin);
        verts[1] = new Vector3(x0, 0, z1); uvs[1] = new Vector2(u0, vMax);
        verts[2] = new Vector3(x1, 0, z0); uvs[2] = new Vector2(u1, vMin);
        verts[3] = new Vector3(x1, 0, z1); uvs[3] = new Vector2(u1, vMax);
        verts[4] = new Vector3(x2, 0, z0); uvs[4] = new Vector2(u2, vMin);
        verts[5] = new Vector3(x2, 0, z1); uvs[5] = new Vector2(u2, vMax);
        verts[6] = new Vector3(x3, 0, z0); uvs[6] = new Vector2(u3, vMin);
        verts[7] = new Vector3(x3, 0, z1); uvs[7] = new Vector2(u3, vMax);

        int[] t = { 0,2,1, 1,2,3,  2,4,3, 3,4,5,  4,6,5, 5,6,7 };
        System.Array.Copy(t, tris, t.Length);

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.name = "ButterflyMesh_" + sprite.name;

        return mesh;
    }
}
