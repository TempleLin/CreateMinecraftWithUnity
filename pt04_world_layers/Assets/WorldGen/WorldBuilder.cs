using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldBuilder : MonoBehaviour {
    /// <summary>
    /// The dimensions determines how many chunks there are gonna be in the world.
    /// </summary>
    [SerializeField] private Vector3 worldDimensions = new Vector3(3, 3, 3);

    /// <summary>
    /// This determines the width, height, and depth of each chunks in the world.
    /// </summary>
    [SerializeField] private Vector3 chunkDimensions = new Vector3(10, 10, 10);

    /// <summary>
    /// Prefab for generating chunks.
    /// </summary>
    [SerializeField] private GameObject chunkPrefab;

    [Header("Perlin Graph Layers")]
    [SerializeField] private PerlinGrapher grassLayer;
    [SerializeField] private PerlinGrapher stoneLayer;
    [SerializeField] private PerlinGrapher diamondTopLayer;
    [SerializeField] private PerlinGrapher diamondBotLayer;
    /// <summary>
    /// Cave uses 3D Perlin Noise to generate.
    /// </summary>
    [SerializeField] private Perlin3DGrapher cave3DLayer;
    /// <summary>
    /// Limiting the height of the cave 3D layer, or else the generated caves might dig up lots of blocks on the surface.
    /// </summary>
    [SerializeField] private PerlinGrapher caveTopLayer;

    [Header("Loading Game")]
    [SerializeField] private Camera loadingCamera;
    [SerializeField] private Slider loadingProgressBar;

    [Header("Player")]
    [SerializeField] private GameObject fpsControllerPrefab;
    [SerializeField] private float playerSpawnDropHeight = 5;
    
    private void Start() {
        if (loadingProgressBar != null) {
            loadingProgressBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
        }
        StartCoroutine(buildWorld());
    }

    /// <summary>
    /// Using "yield return null", Unity will return back to execute this method's contents for each frame.
    /// Thus each chunk gets generated per frame. This will reduce the waiting time for chunk generation. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator buildWorld() {
        // This ChunkBuilder gets built when passed into Chunk prefab's Chunk component.
        ChunkMeshBuilder chunkMeshBuilder = new ChunkMeshBuilder();
        
        for (int z = 0; z < worldDimensions.z; z++) {
            for (int y = 0; y < worldDimensions.y; y++) {
                for (int x = 0; x < worldDimensions.x; x++) {
                    GameObject chunkObj = Instantiate(chunkPrefab);
                    
                    Vector3 position = new Vector3(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);

                    chunkMeshBuilder
                        .setLocation(position)
                        .setDimensions(chunkDimensions)
                        .setGrassLayerAttribs(grassLayer)
                        .setStoneLayerAttribs(stoneLayer)
                        .setDiamondLayersAttribs(diamondTopLayer, diamondBotLayer)
                        .setCaveLayersAttribs(cave3DLayer, caveTopLayer);
                    
                    Chunk chunk = chunkObj.GetComponent<Chunk>();
                    chunk.genChunk(chunkMeshBuilder);

                    // Increase the progress bar per chunk creation.
                    if (loadingProgressBar != null) {
                        loadingProgressBar.value++;
                    }

                    yield return null;
                }
            }
        }

        if (loadingProgressBar != null) {
            loadPlayer();
            deactivateLoadingUtils();
        }
    }

    private void deactivateLoadingUtils() {
        loadingCamera.gameObject.SetActive(false);
        loadingProgressBar.gameObject.SetActive(false);
    }
    private void loadPlayer() {
        Instantiate(fpsControllerPrefab, getPlayerPosition(), Quaternion.identity);
    }

    /// <summary>
    /// Improve workflow automation by calculating player position dynamically in response to the world.
    ///
    /// Player's spawn height in world will be higher than grass with the specified x and y.
    /// </summary>
    /// <returns>Player position.</returns>
    private Vector3 getPlayerPosition() {
        float xPos = (worldDimensions.x * chunkDimensions.x) / 2.0f;
        float zPos = (worldDimensions.y * chunkDimensions.y) / 2.0f;
        float yPos = MeshUtils.fBM(xPos, zPos, grassLayer.Octaves, grassLayer.Scale, grassLayer.HeightScale, 
            grassLayer.HeightOffset) + playerSpawnDropHeight;
        return new Vector3(xPos, yPos, zPos);
    }

    public Vector3 WorldDimensions => worldDimensions;
    public Vector3 ChunkDimensions => chunkDimensions;
}