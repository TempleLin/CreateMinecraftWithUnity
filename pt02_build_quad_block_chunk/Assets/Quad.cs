using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad : MonoBehaviour {
    private Mesh _mesh;
    /**
     * For containing the mesh data. (all vertices' data, such as coordinate, normal, UV, triangle etc.)
     */
    private MeshFilter _meshFilter;
    /**
     * For drawing/rendering the mesh itself.
     */
    private MeshRenderer _meshRenderer;
    void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        _mesh = new Mesh();
        _mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6]; //A quad contains two triangles = 6 vertices.

        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv11 = new Vector2(1, 1);

        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

        vertices = new [] { p4, p5, p1, p0 };
        normals = new [] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        uvs = new [] { uv11, uv01, uv00, uv10 };
        /*
         * Triangle's vertices should be set in clockwise order, so Unity can determine which side is front.
         */
        triangles = new[] { 3, 1, 0, 3, 2, 1 };

        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
    }

    void Update() {
        
    }
}
