using UnityEngine;

/**
 * A chunk represents the largest mesh that you want to have within your world environment.
 * You can't make an absolutely massive world and expect everything to be in the one mesh.
 * 
 * A chunk is made up of blocks.
 */
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour {
    /**
     * Material containing Minecraft texture.
     */
    [SerializeField] private Material atlas;

    /**
     * A chunk should have width, height, and depth.
     */
    private int width = 10; // X coordinate
    private int height = 10; // Y coordinate
    private int depth = 10; // Z coordinate
    
    private PerlinGrapher grassLayer;

    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    /// <summary>
    /// For adding collisions to the chunk.
    /// </summary>
    private MeshCollider _meshCollider;


    /// <summary>
    ///     Gets called from WorldBuilder. Let WorldBuilder decide all the attributes to generate this single chunk.
    ///     The ChunkBuilder object should be configured in WorldBuilder.
    /// </summary>
    /// <param name="chunkMeshBuilder"></param>
    public void genChunk(ChunkMeshBuilder chunkMeshBuilder) {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();
        
        _meshRenderer.material = atlas;

        Mesh newMesh = chunkMeshBuilder.build();

        width = chunkMeshBuilder.Width;
        height = chunkMeshBuilder.Height;
        depth = chunkMeshBuilder.Depth;
        grassLayer = chunkMeshBuilder.GrassLayer;

        _meshFilter.mesh = newMesh;
        _meshCollider.sharedMesh = newMesh;
    }

    public MeshRenderer MeshRenderer => _meshRenderer;
}