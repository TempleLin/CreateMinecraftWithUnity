using UnityEngine;

/**
 * This is just a simple example component for generating a chunk. To generate a chunk formally, use Chunk component. 
 */
public class ChunkExample : MonoBehaviour {
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
    
    [SerializeField] private PerlinGrapher grassLayer;
    [SerializeField] private PerlinGrapher stoneLayer;

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = atlas;

        ChunkMeshBuilder chunkMeshBuilder = new ChunkMeshBuilder();
        Mesh newMesh = chunkMeshBuilder
         .setDimensions(width, height, depth)
         .setGrassLayerAttribs(grassLayer)
         .setStoneLayerAttribs(stoneLayer)
         .build();

        _meshFilter.mesh = newMesh;
    }
}