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
    private readonly int depth = 2; // Z coordinate
    private readonly int height = 2; // Y coordinate

    /**
     * A chunk should have width, height, and depth.
     */
    private readonly int width = 2; // X coordinate
    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    /**
     * Use multi-dimensional array to store the blocks into a chunk.
     * 
     * This can let game know the position of each block (x,y,z), position of a chunk(chunkSize, chunkSize, chunkSize),
     * and positioning the chunks next to each other and not overlap the blocks.
     */
    private Mesh[,,] blocks;

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = atlas;

        BlockBuilder blockBuilder = new BlockBuilder();
        
        blocks = new Mesh[width, height, depth];

        for (int z = 0; z < depth; z++) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    blocks[x, y, z] = blockBuilder.build(new Vector3(x, y, z), MeshUtils.BlockType.DIRT, MeshUtils.BlockType.DIRT);
                }
            }
        }
    }
}