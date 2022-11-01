using UnityEngine;

public class WorldBuilder : MonoBehaviour {
    /// <summary>
    ///     Minecraft should only have "ONE" world. Thus can make this member static.
    /// The dimensions determines how many chunks there are gonna be in the world.
    /// </summary>
    private static Vector3 worldDimensions = new Vector3(3, 3, 3);

    /// <summary>
    /// This determines the width, height, and depth of each chunks in the world.
    /// </summary>
    private static Vector3 chunkDimensions = new Vector3(10, 10, 10);

    /// <summary>
    /// Prefab for generating chunks.
    /// </summary>
    [SerializeField] private GameObject chunkPrefab;
    private void Start() {
        // This ChunkBuilder gets built when passed into Chunk prefab's Chunk component.
        ChunkMeshBuilder chunkMeshBuilder = new ChunkMeshBuilder();
        
        for (int z = 0; z < worldDimensions.z; z++) {
            for (int y = 0; y < worldDimensions.y; y++) {
                for (int x = 0; x < worldDimensions.x; x++) {
                    GameObject chunkObj = Instantiate(chunkPrefab);
                    
                    Vector3 position = new Vector3(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
                    
                    chunkMeshBuilder
                        .setLocation(position)
                        .setDimensions(chunkDimensions);
                    
                    Chunk chunk = chunkObj.GetComponent<Chunk>();
                    chunk.genChunk(chunkMeshBuilder);
                }
            }
        }
    }

    public static Vector3 WorldDimensions => worldDimensions;
    public static Vector3 ChunkDimensions => chunkDimensions;
}