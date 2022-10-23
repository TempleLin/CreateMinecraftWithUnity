using System;
using UnityEngine;

public class Block : MonoBehaviour {
    
    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        QuadBuilder quadBuilder = new QuadBuilder();
        /*
         * Create all quads of a mesh.
         */
        Mesh[] quadsMeshes = {
            quadBuilder.build(BlockSide.BOTTOM, new Vector3(0, 0, 0)),
            quadBuilder.build(BlockSide.TOP, new Vector3(0, 0, 0)),
            quadBuilder.build(BlockSide.LEFT, new Vector3(0, 0, 0)),
            quadBuilder.build(BlockSide.RIGHT, new Vector3(0, 0, 0)),
            quadBuilder.build(BlockSide.FRONT, new Vector3(0, 0, 0)),
            quadBuilder.build(BlockSide.BACK, new Vector3(0, 0, 0)),
        };

        /*
         * Merge all the quad meshes into single mesh through this custom utility method.
         */
        _meshFilter.mesh = MeshUtils.mergeMeshes(quadsMeshes);
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}