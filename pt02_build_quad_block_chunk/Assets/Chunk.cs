using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

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

    /**
     * Use multi-dimensional array to store the blocks into a chunk.
     * 
     * This can let game know the position of each block (x,y,z), position of a chunk(chunkSize, chunkSize, chunkSize),
     * and positioning the chunks next to each other and not overlap the blocks.
     */
    private Mesh[,,] blockMeshes;

    /**
     * Block type of all blocks in chunk. Uses single loop for faster processing.
     *
     * (Flat[x + WIDTH * (Y + DEPTH *z)] = Original[x, y, z])
     */
    private MeshUtils.BlockType[] chunkData;
    
    /// <summary>
    /// This is the essential function to do the landscaping. It configures all blocks' data in chunk.
    /// </summary>
    private void buildChunk() {
        int blockCount = width * depth * height;
        chunkData = new MeshUtils.BlockType[blockCount];
        for (int i = 0; i < blockCount; i++) {
            if (UnityEngine.Random.Range(0, 100) < 50)
                chunkData[i] = MeshUtils.BlockType.DIRT;
            else
                chunkData[i] = MeshUtils.BlockType.AIR;
        }
    }

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = atlas;

        BlockBuilder blockBuilder = new BlockBuilder();
        
        blockMeshes = new Mesh[width, height, depth];
        
        buildChunk();

        List<Mesh> inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triStart = 0;
        int meshCount = width * height * depth;
        int m = 0;
        ProcessMeshDataJob jobs = new ProcessMeshDataJob();
        
        /*
         * Building block meshes into chunk is a slow process. To boost up the performance, this project uses Burst compiler and Job system.  
         */
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (int z = 0; z < depth; z++) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Mesh blockMesh;
                    MeshUtils.BlockType blockType = chunkData[x + width * (y + depth * z)];
                    
                    /*
                     * Block mesh doesn't need to be built if it's air. Make it null instead.
                     */
                    blockMesh = blockType is MeshUtils.BlockType.AIR ? 
                        null : blockBuilder.build(this, new Vector3(x, y, z), blockType);

                    blockMeshes[x, y, z] = blockMesh;
                    if (blockMesh != null) {
                        inputMeshes.Add(blockMesh);

                        jobs.vertexStart[m] = vertexStart;
                        jobs.triStart[m] = triStart;
                    
                        int vCount = blockMesh.vertexCount;
                        /*
                         * "0" mesh sub-mesh 0. In this project, a block mesh only has the main mesh itself, no other sub-meshes.
                         *
                         * Index count is the count of indices (index buffer) used in the mesh.
                         */
                        int iCount = (int) blockMesh.GetIndexCount(0);
                    
                        vertexStart += vCount;
                        triStart += iCount;
                        m++;
                    }
                }
            }
        }

        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        /*
         * Allocates data structures for Mesh creation using C# Jobs.
         *
         * "1" is the mesh count that will be created.
         */
        Mesh.MeshDataArray outputMeshData = Mesh.AllocateWritableMeshData(1);
        
        jobs.outputMesh = outputMeshData[0];
        /*
         * Sets the index buffer size and format of the Mesh that Unity creates from the MeshData.
         * These assigned buffer params will be used in ProcessMeshDataJob.Execute() below ("outputMesh.GetIndexData").
         * "triStart" has been incremented on top and finished at the last one, now it's used as the size of total indices.
         */
        jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
        
        /*
         * These assigned streams will be used in ProcessMeshDataJob.Execute() below ("outputMesh.GetVertexData").
         * "vertexStart" has been incremented on top and finished at the last one, now it's used as the size of total vertices.
         */
        jobs.outputMesh.SetVertexBufferParams(vertexStart,
            new VertexAttributeDescriptor(VertexAttribute.Position), // Position: vertex.
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2)); // TexCoord0: First UV in shader.

        /*
         * "4": Count of jobs to do.
         */
        JobHandle handle = jobs.Schedule(inputMeshes.Count, 4);
        Mesh newMesh = new Mesh();
        newMesh.name = "Chunk";
        SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
        subMeshDescriptor.firstVertex = 0;
        subMeshDescriptor.vertexCount = vertexStart;
        
        /*
         * Wait for jobs to complete.
         */
        handle.Complete();
        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, subMeshDescriptor);
        
        
        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new []{ newMesh });
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();
        
        /*
         * Recalculate colliders.
         */
        newMesh.RecalculateBounds();

        _meshFilter.mesh = newMesh;
    }
    
    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor {
        /**
         * Will contain all the incoming meshes data for merging.
         *
         * Mesh.MeshDataArray type: An array of Mesh data snapshots for C# Job System access.
         */
        [ReadOnly] public Mesh.MeshDataArray meshData;

        /**
         * The mesh to build into. (by merging meshes)
         */
        public Mesh.MeshData outputMesh;

        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        public void Execute(int index) {
            Mesh.MeshData data = meshData[index];
            int vCount = data.vertexCount;
            int vStart = vertexStart[index];

            NativeArray<float3> verts =
                new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            /*
             * Get mesh data's vertices and put into the verts native array.
             */
            data.GetVertices(verts.Reinterpret<Vector3>());
            
            /*
             * Normals array will be the same size as verts array. Since every vertex contains a normal.
             */
            NativeArray<float3> normals = 
                new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());
            
            /*
             *  With Unity's job system, if trying to get the data out using Vector2 instead of Vector3,
             * will produce weird results. (Not sure if it's changed yet, needs further confirmation.)
             */
            NativeArray<float3> uvs = 
                new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            /*
             *  First arg is the channel. A vertex can contain multiple UVs. Multi-channels will be used later
             * in the course (such as cracks on a block when trying to mine them in Minecraft) 
             */
            data.GetUVs(0, uvs.Reinterpret<Vector3>());

            /*
             *  MeshData container contains streams within (configured in code above). First stream is the
             * vertices; second is the normal; third is the UVs.
             *
             *  These buffer arrays will be set with all the vertices, normals, and UVs data needed for the
             * final merged output mesh during execution below.
             */
            NativeArray<Vector3> outputVerts = outputMesh.GetVertexData<Vector3>();
            NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            NativeArray<Vector3> outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);

            for (int i = 0; i < vCount; i++) {
                outputVerts[i + vStart] = verts[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
            }

            /*
             * Native arrays are unmanaged. Disposing them manually is required.
             */
            verts.Dispose();
            normals.Dispose();
            uvs.Dispose();


            int tStart = triStart[index];
            /*
             * A block mesh only contains one mesh, no other sub-meshes. Consequently, the index to pass should be 0.
             */
            int tCount = data.GetSubMesh(0).indexCount;
            /*
             * This buffer array is configured above (SetIndexBufferParams).
             *  This buffer array will be set with triangle indices data needed for the final merged output mesh during
             * execution below.
             */
            NativeArray<int> outputTris = outputMesh.GetIndexData<int>(); // Triangles of the output mesh.
            
            /*
             * Some platforms use UInt16 (ex. Android), some UInt32 (ex. Desktop).
             */
            if (data.indexFormat == IndexFormat.UInt16) {
                NativeArray<ushort> tris = data.GetIndexData<ushort>(); // Get triangles indices of the current mesh in process.
                for (int i = 0; i < tCount; i++) {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            } else { //UInt32
                NativeArray<int> tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; i++) {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
        }
    }

    public int Width => width;
    public int Height => height;
    public int Depth => depth;
    
    public MeshUtils.BlockType[] ChunkData => chunkData;
}