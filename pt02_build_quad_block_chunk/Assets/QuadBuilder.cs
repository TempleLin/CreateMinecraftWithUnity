using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadBuilder {
    /**
     *  This will build a quad mesh, so that a gameobject can assign the mesh to its MeshFilter component,
     * which attaches all vertices' data and make a complete mesh data.
     */
    public Mesh build() {
        Mesh _mesh = new Mesh();
        _mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6]; //A quad contains two triangles = 6 vertices.

        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv11 = new Vector2(1, 1);

        /*
         * All these vertices' coordinates make up a whole block. only four vertices will be used to create a quad facing forward.
         */
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
         * A triangle's vertices should be set in clockwise order, so Unity can determine which side is front.
         */
        triangles = new[] { 3, 1, 0, 3, 2, 1 };

        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        
        _mesh.RecalculateBounds();
        return _mesh;
    }

    /**
     * Call this if the target mesh is to build a quad that's part of a complete block, instead of just one quad as mesh.
     */
    public Mesh build(BlockSide blockSide, Vector3 offset) {
     Mesh _mesh = new Mesh();
     _mesh.name = "ScriptedQuad";

     Vector3[] vertices = new Vector3[4];
     Vector3[] normals = new Vector3[4];
     Vector2[] uvs = new Vector2[4];
     int[] triangles = new int[6];

     Vector2 uv00 = new Vector2(0, 0);
     Vector2 uv10 = new Vector2(1, 0);
     Vector2 uv01 = new Vector2(0, 1);
     Vector2 uv11 = new Vector2(1, 1);

     Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f) + offset;
     Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f) + offset;
     Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset;
     Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset;
     Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f) + offset;
     Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f) + offset;
     Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset;
     Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset;

     switch (blockSide) {
      case BlockSide.FRONT:
       vertices = new [] { p4, p5, p1, p0 };
       normals = new [] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.BACK:
       vertices = new [] { p6, p7, p3, p2 };
       normals = new [] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.BOTTOM:
       vertices = new[] { p0, p1, p2, p3 };
       normals = new[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.TOP:
       vertices = new[] { p7, p6, p5, p4 };
       normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.LEFT:
       vertices = new[] { p7, p4, p0, p3 };
       normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.RIGHT:
       vertices = new[] { p5, p6, p2, p1 };
       normals = new[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
     }

     triangles = new[] { 3, 1, 0, 3, 2, 1 }; //These don't need to change, bc they're referring to the "vertices" array.

     _mesh.vertices = vertices;
     _mesh.normals = normals;
     _mesh.uv = uvs;
     _mesh.triangles = triangles;
        
     _mesh.RecalculateBounds();
     return _mesh;
    }

    /**
     *  Call this when if the block uses a texture, and needs to assign specific UV of the image to texture.
     *  Minecraft's single texture image contains multiple block image parts separated evenly (0.0625 * 0.0625).
     * A block object should only use one block image part in texture, by setting four corners of UV.
     */
    public Mesh build(BlockSide blockSide, Vector3 offset, MeshUtils.BlockType blockType) {
          Mesh _mesh = new Mesh();
     _mesh.name = "ScriptedQuad";

     Vector3[] vertices = new Vector3[4];
     Vector3[] normals = new Vector3[4];
     Vector2[] uvs = new Vector2[4];
     int[] triangles = new int[6];

     Vector2 uv00 = MeshUtils.blockUVs[(int)blockType, 0];
     Vector2 uv10 = MeshUtils.blockUVs[(int)blockType, 1];
     Vector2 uv01 = MeshUtils.blockUVs[(int)blockType, 2];
     Vector2 uv11 = MeshUtils.blockUVs[(int)blockType, 3];

     Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f) + offset;
     Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f) + offset;
     Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset;
     Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset;
     Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f) + offset;
     Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f) + offset;
     Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset;
     Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset;

     switch (blockSide) {
      case BlockSide.FRONT:
       vertices = new [] { p4, p5, p1, p0 };
       normals = new [] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.BACK:
       vertices = new [] { p6, p7, p3, p2 };
       normals = new [] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.BOTTOM:
       vertices = new[] { p0, p1, p2, p3 };
       normals = new[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.TOP:
       vertices = new[] { p7, p6, p5, p4 };
       normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.LEFT:
       vertices = new[] { p7, p4, p0, p3 };
       normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
      case BlockSide.RIGHT:
       vertices = new[] { p5, p6, p2, p1 };
       normals = new[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
       uvs = new [] { uv11, uv01, uv00, uv10 };
       break;
     }

     triangles = new[] { 3, 1, 0, 3, 2, 1 }; //These don't need to change, bc they're referring to the "vertices" array.

     _mesh.vertices = vertices;
     _mesh.normals = normals;
     _mesh.uv = uvs;
     _mesh.triangles = triangles;
        
     _mesh.RecalculateBounds();
     return _mesh;
    }
}
