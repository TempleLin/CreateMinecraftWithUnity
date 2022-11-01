using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///     A chunk represents the largest mesh that you want to have within your world environment.
///     You can't make an absolutely massive world and expect everything to be in the one mesh.
///     A chunk is made up of blocks.
/// </summary>
public class ChunkBuilder {
    /**
     * Use multi-dimensional array to store the blocks into a chunk.
     * 
     * This can let game know the position of each block (x,y,z), position of a chunk(chunkSize, chunkSize, chunkSize),
     * and positioning the chunks next to each other and not overlap the blocks.
     */
    private Mesh[,,] blockMeshes;

    public ChunkBuilder(int width, int height, int depth) {
        Width = width;
        Height = height;
        Depth = depth;
    }

    /**
     * A chunk should have width, height, and depth.
     */
    public int Width { get; }

    public int Height { get; }

    public int Depth { get; }

    /// Block type of all blocks in chunk. Uses single loop for faster processing.
    /// 
    /// formulas:
    ///     Flat[x + WIDTH * (Y + DEPTH *z)] = Original[x, y, z]
    ///     x = i % WIDTH
    ///     y = (i / WIDTH) % HEIGHT
    ///     z = i / (WIDTH * HEIGHT)
    ///  
    /// Make sure that size of this array matches the total count of blocks in chunk
    /// with specified width, height, and depth.
    /// 
    /// This member gets set when .build() gets called. As needed content blocks might
    /// be different everytime a new chunk gets built.
    public MeshUtils.BlockType[] BlocksTypes { get; private set; }
    
    /// <summary>
    ///     This is the essential function to do the landscaping. It configures all blocks' data in chunk.
    /// Generating heights of the chunk through the use of Perlin Noise with Fractional Brownian Motion
    /// is also configured here.
    /// </summary>
    private void buildChunk() {
        var blockCount = Width * Depth * Height;
        BlocksTypes = new MeshUtils.BlockType[blockCount];
        for (int i = 0; i < blockCount; i++) {
            int x = i % Width;
            int y = (i / Width) % Height;
            int z = i / (Width * Height);
            if (MeshUtils.fBM(x, z, 8, 0.001f, 10, -33) > y) {
                BlocksTypes[i] = MeshUtils.BlockType.DIRT;
            } else {
                BlocksTypes[i] = MeshUtils.BlockType.AIR;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="blocksTypes">
    ///     All blocks needed to be built in the chunk.
    /// </param>
    /// <returns></returns>
    public Mesh build() {
        BlockBuilder blockBuilder = new BlockBuilder();

        blockMeshes = new Mesh[Width, Height, Depth];

        buildChunk();

        List<Mesh> inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triStart = 0;
        int meshCount = Width * Height * Depth;
        int m = 0;
        ProcessMeshDataJob jobs = new ProcessMeshDataJob {
            /*
         * Building block meshes into chunk is a slow process. To boost up the performance, this project uses Burst compiler and Job system.  
         */
            vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
            triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
        };

        for (int z = 0; z < Depth; z++)
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++) {
            MeshUtils.BlockType blockType = BlocksTypes[x + Width * (y + Depth * z)];

            /*
                     * Block mesh doesn't need to be built if it's air. Make it null instead.
                     */
            Mesh blockMesh = blockType is MeshUtils.BlockType.AIR
                ? null
                : blockBuilder.build(this, new Vector3(x, y, z), blockType);

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
                int iCount = (int)blockMesh.GetIndexCount(0);

                vertexStart += vCount;
                triStart += iCount;
                m++;
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
        SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, triStart);
        subMeshDescriptor.firstVertex = 0;
        subMeshDescriptor.vertexCount = vertexStart;

        /*
         * Wait for jobs to complete.
         */
        handle.Complete();
        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, subMeshDescriptor);


        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] { newMesh });
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();

        /*
         * Recalculate colliders.
         */
        newMesh.RecalculateBounds();

        return newMesh;
    }

    [BurstCompile]
    private struct ProcessMeshDataJob : IJobParallelFor {
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

            NativeArray<float3> verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            /*
             * Get mesh data's vertices and put into the verts native array.
             */
            data.GetVertices(verts.Reinterpret<Vector3>());

            /*
             * Normals array will be the same size as verts array. Since every vertex contains a normal.
             */
            NativeArray<float3> normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());

            /*
             *  With Unity's job system, if trying to get the data out using Vector2 instead of Vector3,
             * will produce weird results. (Not sure if it's changed yet, needs further confirmation.)
             */
            NativeArray<float3> uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
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
            NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(1);
            NativeArray<Vector3> outputUVs = outputMesh.GetVertexData<Vector3>(2);

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
            } else {
                //UInt32
                NativeArray<int> tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; i++) {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
        }
    }
}