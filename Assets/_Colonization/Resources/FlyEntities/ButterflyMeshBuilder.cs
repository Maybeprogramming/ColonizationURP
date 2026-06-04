using System.Collections.Generic;
using UnityEngine;

public static class ButterflyMeshBuilder
{
    private const float HalfHeight = 0.5f;
    private const float QuadDivisions = 3f;
    private const float WidthRatio = 2f / 3f;

    private static readonly Dictionary<Sprite, Mesh> MeshCache = new();

    public static Mesh BuildMesh(Sprite sprite)
    {
        if (MeshCache.TryGetValue(sprite, out Mesh cachedMesh))
            return cachedMesh;

        Mesh mesh = BuildInternal(sprite);
        MeshCache[sprite] = mesh;
        return mesh;
    }

    private static Mesh BuildInternal(Sprite sprite)
    {
        Rect rect = sprite.rect;
        float aspect = rect.width / rect.height;
        float halfWidth = HalfHeight * aspect;

        float quadWidth = halfWidth * WidthRatio;

        float xLeft = -halfWidth;
        float xMidLeft = -halfWidth + quadWidth;
        float xMidRight = -halfWidth + quadWidth * 2f;
        float xRight = halfWidth;
        float zBottom = -HalfHeight;
        float zTop = HalfHeight;

        Vector3[] vertices = new Vector3[8];
        Vector2[] uvCoordinates = new Vector2[8];
        int[] triangles = new int[18];

        Texture2D texture = sprite.texture;
        Rect textureRect = sprite.textureRect;

        float uvMinX = textureRect.xMin / texture.width;
        float uvMaxX = textureRect.xMax / texture.width;
        float uvMinY = textureRect.yMin / texture.height;
        float uvMaxY = textureRect.yMax / texture.height;

        float u0 = uvMinX;
        float u1 = Mathf.Lerp(uvMinX, uvMaxX, 1f / QuadDivisions);
        float u2 = Mathf.Lerp(uvMinX, uvMaxX, 2f / QuadDivisions);
        float u3 = uvMaxX;

        vertices[0] = new Vector3(xLeft, 0f, zBottom);
        uvCoordinates[0] = new Vector2(u0, uvMinY);

        vertices[1] = new Vector3(xLeft, 0f, zTop);
        uvCoordinates[1] = new Vector2(u0, uvMaxY);

        vertices[2] = new Vector3(xMidLeft, 0f, zBottom);
        uvCoordinates[2] = new Vector2(u1, uvMinY);

        vertices[3] = new Vector3(xMidLeft, 0f, zTop);
        uvCoordinates[3] = new Vector2(u1, uvMaxY);

        vertices[4] = new Vector3(xMidRight, 0f, zBottom);
        uvCoordinates[4] = new Vector2(u2, uvMinY);

        vertices[5] = new Vector3(xMidRight, 0f, zTop);
        uvCoordinates[5] = new Vector2(u2, uvMaxY);

        vertices[6] = new Vector3(xRight, 0f, zBottom);
        uvCoordinates[6] = new Vector2(u3, uvMinY);

        vertices[7] = new Vector3(xRight, 0f, zTop);
        uvCoordinates[7] = new Vector2(u3, uvMaxY);

        int[] triangleIndices = { 0, 2, 1, 1, 2, 3, 2, 4, 3, 3, 4, 5, 4, 6, 5, 5, 6, 7 };
        System.Array.Copy(triangleIndices, triangles, triangleIndices.Length);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvCoordinates;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.name = "ButterflyMesh_" + sprite.name;

        return mesh;
    }
}
