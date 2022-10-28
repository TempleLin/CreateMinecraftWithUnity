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

        BlockBuilder blockBuilder = new BlockBuilder();
        Mesh mesh = blockBuilder.build(new Vector3(0, 0, 0));
        
        _meshFilter.mesh = mesh;
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}