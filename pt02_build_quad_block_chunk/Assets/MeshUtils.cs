

using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

/*
 * VertexData: For containing coordinates, UVs, normals.
 */

public static class MeshUtils {
    
    public enum BlockType {
        // A grass has green top and dirt side/bottom.
        GRASSTOP, GRASSSIDE, DIRT, WATER, STONE, SAND
    }

    /**
     * UV positions of different types of blocks in the texture. Each block's UV size is 0.0625 * 0.0625. 
     */
    public static Vector2[,] blockUVs = { {
            // Grass Top
            new Vector2(0.125f, 0.375f), new Vector2(0.1875f, 0.375f),
            new Vector2(0.125f, 0.4375f), new Vector2(0.1875f, 0.4375f)
        }, { 
            // Grass Side
            new Vector2(0.1875f, 0.9375f), new Vector2(0.25f, 0.9375f),
            new Vector2(0.1875f, 1.0f), new Vector2(0.25f, 1.0f)
        }, {
            // Dirt
            new Vector2(0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f),
            new Vector2(0.125f, 1.0f), new Vector2(0.1875f, 1.0f)
        }, {
            // Water
            new Vector2(0.875f, 0.125f), new Vector2(0.9375f, 0.125f),
            new Vector2(0.875f, 0.1875f), new Vector2(0.9375f, 0.1875f)
        }, {
            // Stone
            new Vector2(0.0f, 0.875f), new Vector2(0.0625f, 0.875f),
            new Vector2(0.0f, 0.9375f), new Vector2(0.0625f, 0.9375f)
        }, {
            // Sand
            new Vector2(0.125f, 0.875f), new Vector2(0.1875f, 0.875f),
            new Vector2(0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f)
        }
    };
    
    /**
     * In this example project, this will be used to merge quad meshes to create a cube.
     */
    public static Mesh mergeMeshes(Mesh[] meshes) {
        Mesh mesh = new Mesh();
        
        /*
         * pointsOrder: Keep track of order in which vertices occur.
         */
        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        /*
         * HashSet is a hashtable collection containing non-repeating elements.
         */
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> triangles = new List<int>();

        int pIndex = 0;
        for (int i = 0; i < meshes.Length; i++) {
            if (meshes[i] == null) continue;
            for (int j = 0; j < meshes[i].vertices.Length; j++) {
                Vector3 v = meshes[i].vertices[j];
                Vector3 n = meshes[i].normals[j];
                Vector2 u = meshes[i].uv[j];
                VertexData p = new VertexData(v, n, u);
                /*
                 * Only add the vertex data if it's not included/repeated yet.
                 */
                if (!pointsHash.Contains(p)) {
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);

                    pIndex++;
                }
            }

            /*
             *  Triangles' indices need to be re-calculated and added, so that they follow new order of merged and
             * non-repeating vertices.
             */
            for (int t = 0; t < meshes[i].triangles.Length; t++) {
                int triPoint = meshes[i].triangles[t];
                
                Vector3 v = meshes[i].vertices[triPoint];
                Vector3 n = meshes[i].normals[triPoint];
                Vector2 u = meshes[i].uv[triPoint];
                VertexData p = new VertexData(v, n, u);

                pointsOrder.TryGetValue(p, out int index);
                triangles.Add(index);
            }

            meshes[i] = null;
        }
        
        extractArrays(pointsOrder, mesh);
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }

    /**
     * Extract collections of vertex data into individual coordinate, normal, and UV arrays, and assign them into mesh.
     */
    public static void extractArrays(Dictionary<VertexData, int> list, Mesh mesh) {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (VertexData v in list.Keys) {
            verts.Add(v.Item1);
            norms.Add(v.Item2);
            uvs.Add(v.Item3);
        }

        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
    }
}