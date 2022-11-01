using UnityEngine;

/**
 * A chunk represents the largest mesh that you want to have within your world environment.
 * You can't make an absolutely massive world and expect everything to be in the one mesh.
 * 
 * A chunk is made up of blocks.
 */
public class Chunk : MonoBehaviour {
    /**
     * Material containing Minecraft texture.
     */
    [SerializeField] private Material atlas;

    /**
     * A chunk should have width, height, and depth.
     */
    [SerializeField] private int width = 2; // X coordinate

    [SerializeField] private int height = 2; // Y coordinate
    [SerializeField] private int depth = 2; // Z coordinate

    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;

    private MeshRenderer _meshRenderer;

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = atlas;

        var chunkBuilder = new ChunkBuilder(width, height, depth);
        var newMesh = chunkBuilder.build();

        _meshFilter.mesh = newMesh;
    }
}