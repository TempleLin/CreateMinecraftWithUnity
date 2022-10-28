using System;
using UnityEngine;

/**
 * A chunk represents the largest mesh that you want to have within your world environment.
 * You can't make an absolutely massive world and expect everything to be in the one mesh.
 *
 * A chunk is made up of blocks.
 */
public class Chunk : MonoBehaviour {
    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    /**
     * Material containing Minecraft texture.
     */
    [SerializeField] private Material atlas;
    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        QuadBuilder quadBuilder = new QuadBuilder();
        /*
         * Create all quads of a mesh.
         */
        Mesh[] quadsMeshes = {
            quadBuilder.build(MeshUtils.BlockSide.BOTTOM, new Vector3(0, 0, 0)),
            quadBuilder.build(MeshUtils.BlockSide.TOP, new Vector3(0, 0, 0)),
            quadBuilder.build(MeshUtils.BlockSide.LEFT, new Vector3(0, 0, 0)),
            quadBuilder.build(MeshUtils.BlockSide.RIGHT, new Vector3(0, 0, 0)),
            quadBuilder.build(MeshUtils.BlockSide.FRONT, new Vector3(0, 0, 0)),
            quadBuilder.build(MeshUtils.BlockSide.BACK, new Vector3(0, 0, 0)),
        };

        /*
         * Merge all the quad meshes into single mesh through this custom utility method.
         */
        _meshFilter.mesh = MeshUtils.mergeMeshes(quadsMeshes);
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}