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

    [Header("Perlin Settings")] 
    [SerializeField] private float heightScale = 10;
    [SerializeField] private float scale = 0.001f;
    [SerializeField] private int octaves = 8;
    [SerializeField] private float heightOffset = -33;

    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;

    private MeshRenderer _meshRenderer;

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = atlas;

        ChunkBuilder chunkBuilder = new ChunkBuilder();
        Mesh newMesh = chunkBuilder
         .setDimensions(width, height, depth)
         .setPerlinAttribs(heightScale, scale, octaves, heightOffset)
         .build();

        _meshFilter.mesh = newMesh;
    }
}