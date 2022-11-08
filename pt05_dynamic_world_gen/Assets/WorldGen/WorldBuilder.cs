using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Chunks are generated and sorted in columns in the Y coordinates. The count of chunks in a column depends on
///     worldDimensions.y value.
///     Chunks columns around the player get generated or hidden in respond to player's position in the world.
/// </summary>
public class WorldBuilder : MonoBehaviour {
    /// <summary>
    ///     The dimensions determines how many chunks there are gonna be in the world.
    /// </summary>
    [SerializeField] private Vector3 worldDimensions = new Vector3(3, 3, 3);

    /// <summary>
    ///     This determines the width, height, and depth of each chunks in the world.
    /// </summary>
    [SerializeField] private Vector3 chunkDimensions = new Vector3(10, 10, 10);

    /// <summary>
    ///     Prefab for generating chunks.
    /// </summary>
    [SerializeField] private GameObject chunkPrefab;

    [Header("Perlin Graph Layers")] [SerializeField]
    private PerlinGrapher grassLayer;
    [SerializeField] private PerlinGrapher stoneLayer;
    [SerializeField] private PerlinGrapher diamondTopLayer;
    [SerializeField] private PerlinGrapher diamondBotLayer;
    /// <summary>
    ///     Cave uses 3D Perlin Noise to generate.
    /// </summary>
    [SerializeField] private Perlin3DGrapher cave3DLayer;
    /// <summary>
    ///     Limiting the height of the cave 3D layer, or else the generated caves might dig up lots of blocks on the surface.
    /// </summary>
    [SerializeField] private PerlinGrapher caveTopLayer;

    [Header("Loading Game")] [SerializeField]
    private Camera loadingCamera;
    [SerializeField] private Slider loadingProgressBar;

    [Header("Player")] [SerializeField] private GameObject fpsControllerPrefab;
    [SerializeField] private float playerSpawnDropHeight = 5;

    /// <summary>
    ///     Radius for generating chunks columns near player per world update.
    ///     (For example, if the value is 3, then 3x3 chunks columns near player will get generated if they haven't yet.)
    /// </summary>
    [SerializeField] private int chunkColumnsGenRadius = 3;

    /// <summary>
    /// World updating shouldn't be per frame. Or it'll eat up the PC's resources.
    /// </summary>
    [SerializeField] private float worldUpdateSecsInterval = 0.5f;

    /// <summary>
    /// Gets set with worldUpdateDelay member value on start.
    /// </summary>
    private WaitForSeconds WorldUpdateWaitForSeconds;

    /// <summary>
    ///     This is a queue that will hold and run all IEnumerator actions waiting to be processed frame by frame.
    /// </summary>
    private readonly Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();

    /// <summary>
    ///     These are properties to keep track of when choosing to or not to generate/hide chunks around player in the world.
    /// </summary>
    private readonly HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private readonly HashSet<Vector2Int> chunksColumns = new HashSet<Vector2Int>();

    /// <summary>
    ///     Variable for keeping track of last time the player's position when chunks columns around are being updated
    ///     (generated or hidden).
    /// </summary>
    private Vector3Int lastPlayerPosition;

    /// <summary>
    ///     Is set when the prefab is instantiated. (during loadPlayer())
    /// </summary>
    private GameObject player;
    

    private void Start() {
        WorldUpdateWaitForSeconds = new WaitForSeconds(worldUpdateSecsInterval);
        if (loadingProgressBar != null) 
            loadingProgressBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
        StartCoroutine(buildWorld());
    }

    /// <summary>
    ///     This is the main execution that will keep running all in-waiting world generation/hiding context frame by frame.
    /// </summary>
    /// <returns></returns>
    private IEnumerator buildCoordinator() {
        while (true) {
            while (buildQueue.Count > 0)
                yield return StartCoroutine(buildQueue.Dequeue());
            yield return null;
        }
    }

    /// <summary>
    ///     Using "yield return null", Unity will return back to execute this method's contents for each frame.
    ///     Thus each chunk gets generated per frame. This will reduce the waiting time for chunk generation.
    /// </summary>
    /// <returns></returns>
    private IEnumerator buildWorld() {
        // This ChunkBuilder gets built when passed into Chunk prefab's Chunk component.
        for (int z = 0; z < worldDimensions.z; z++) {
            for (int x = 0; x < worldDimensions.x; x++) {

                buildChunksColumn(x * (int)chunkDimensions.x, z * (int)chunkDimensions.z);

                // Increase the progress bar per "chunks column" creation.
                if (loadingProgressBar != null) loadingProgressBar.value++;
                yield return null;
            }
        }

        if (loadingProgressBar != null) {
            loadPlayer();
            deactivateLoadingUtils();
        }

        lastPlayerPosition = Vector3Int.CeilToInt(getPlayerSpawnPosition());

        StartCoroutine(buildCoordinator());
        StartCoroutine(updateWorld());
    }

    /// <summary>
    ///     Context for chunks columns generate or hide around player are executed here.
    /// </summary>
    /// <returns></returns>
    private IEnumerator updateWorld() {
        while (true) {
            // If the player has moved more than the a chunk's x length.
            // (Make sure the x and z length of a chunk are the same, or this might produce unwanted results.)
            if ((lastPlayerPosition - player.transform.position).magnitude > chunkDimensions.x) {
                Vector3 playerPos = player.transform.position;
                lastPlayerPosition = Vector3Int.CeilToInt(playerPos);
                /*
                 * For example, if player is at x = 27, and a chunk's dimension's x length is 10.
                 * Round(27/10) * 10 = 2 * 10 = 20.
                 * First chunk's starting position is 0; second starts at 10; third chunk starts at 20.
                 * In this example, since the result is 20, the third chunk in the x-axis will be generated.
                 */
                int posX = (int)(Mathf.Round(playerPos.x / chunkDimensions.x) * chunkDimensions.x);
                int posZ = (int)(Mathf.Round(playerPos.z / chunkDimensions.z) * chunkDimensions.z);

                buildQueue.Enqueue(buildRecursiveWorld(posX, posZ, chunkColumnsGenRadius));
                buildQueue.Enqueue(hideColumns(posX, posZ));
            }

            yield return worldUpdateSecsInterval;
        }
    }

    private IEnumerator hideColumns(int x, int z) {
        void hideChunkColumn(int x, int z) {
            for (int y = 0; y < worldDimensions.y; y++) {
                Vector3Int pos = new Vector3Int(x, y * (int)chunkDimensions.y, z);
                if (chunkChecker.Contains(pos)) chunks[pos].gameObject.SetActive(false);
            }
        }

        Vector2Int fpcPos = new Vector2Int(x, z);
        foreach (Vector2Int cc in chunksColumns)
            if ((cc - fpcPos).magnitude >= chunkColumnsGenRadius * chunkDimensions.x)
                hideChunkColumn(cc[0], cc[1]);
        yield return null;
    }

    /// <summary>
    ///     Recursively build all the required chunks columns nearby the radius of given x and y.
    ///     (Think of it as expanding and circling around the given position to generate nearby chunks columns. until reached
    ///     the given radius size.)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="rad"></param>
    /// <returns></returns>
    private IEnumerator buildRecursiveWorld(int x, int z, int rad) {
        int nextrad = rad - 1;
        if (rad <= 0) yield break;

        buildChunksColumn(x, z + (int)chunkDimensions.z);
        buildQueue.Enqueue(buildRecursiveWorld(x, z + (int)chunkDimensions.z, nextrad));
        yield return null;

        buildChunksColumn(x, z - (int)chunkDimensions.z);
        buildQueue.Enqueue(buildRecursiveWorld(x, z - (int)chunkDimensions.z, nextrad));
        yield return null;

        buildChunksColumn(x + (int)chunkDimensions.x, z);
        buildQueue.Enqueue(buildRecursiveWorld(x + (int)chunkDimensions.x, z, nextrad));
        yield return null;

        buildChunksColumn(x - (int)chunkDimensions.x, z);
        buildQueue.Enqueue(buildRecursiveWorld(x - (int)chunkDimensions.x, z, nextrad));
        yield return null;
    }

    /// <summary>
    ///     Chunks are counted and built in "columns". A column contains multiple chunks in the Y coordinate, and the count of
    ///     chunks
    ///     depends on the worldDimensions.y.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    private void buildChunksColumn(int x, int z) {
        for (int y = 0; y < worldDimensions.y; y++) {
            GameObject chunkObj = Instantiate(chunkPrefab);

            // Note that x and z shouldn't be multiplying with chunk dimension here.
            Vector3 position = new Vector3(x, y * chunkDimensions.y, z);
            Vector3Int _position = Vector3Int.CeilToInt(position);
            if (!chunkChecker.Contains(_position)) {
                ChunkMeshBuilder chunkMeshBuilder = new ChunkMeshBuilder()
                    .setLocation(position)
                    .setDimensions(chunkDimensions)
                    .setGrassLayerAttribs(grassLayer)
                    .setStoneLayerAttribs(stoneLayer)
                    .setDiamondLayersAttribs(diamondTopLayer, diamondBotLayer)
                    .setCaveLayersAttribs(cave3DLayer, caveTopLayer);

                Chunk chunk = chunkObj.GetComponent<Chunk>();
                chunk.genChunk(chunkMeshBuilder);

                chunkChecker.Add(_position);
                chunks.Add(_position, chunk);
            } else {
                chunks[_position].gameObject.SetActive(true);
            }
        }

        chunksColumns.Add(new Vector2Int(x, z));
    }

    private void deactivateLoadingUtils() {
        loadingCamera.gameObject.SetActive(false);
        loadingProgressBar.gameObject.SetActive(false);
    }

    private void loadPlayer() {
        player = Instantiate(fpsControllerPrefab, getPlayerSpawnPosition(), Quaternion.identity);
    }

    /// <summary>
    ///     Improve workflow automation by calculating player position dynamically in response to the world.
    ///     Player's spawn height in world will be higher than grass with the specified x and y.
    /// </summary>
    /// <returns>Player position.</returns>
    private Vector3 getPlayerSpawnPosition() {
        float xPos = worldDimensions.x * chunkDimensions.x / 2.0f;
        float zPos = worldDimensions.y * chunkDimensions.y / 2.0f;
        float yPos = MeshUtils.fBM(xPos, zPos, grassLayer.Octaves, grassLayer.Scale, grassLayer.HeightScale,
            grassLayer.HeightOffset) + playerSpawnDropHeight;
        return new Vector3(xPos, yPos, zPos);
    }
    
    public Vector3 WorldDimensions => worldDimensions;
    public Vector3 ChunkDimensions => chunkDimensions;
}