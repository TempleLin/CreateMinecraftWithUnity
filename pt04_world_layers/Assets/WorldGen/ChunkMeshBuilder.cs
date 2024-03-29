﻿using System.Collections.Generic;
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
public class ChunkMeshBuilder {
    /**
     * Use multi-dimensional array to store the blocks into a chunk.
     * 
     * This can let game know the position of each block (x,y,z), position of a chunk(chunkSize, chunkSize, chunkSize),
     * and positioning the chunks next to each other and not overlap the blocks.
     */
    private Mesh[,,] blockMeshes;

    /**
     * A chunk should have width, height, and depth.
     */
    private int width = 2;
    private int height = 2;
    private int depth = 2;

    /// <summary>
    ///     A chunk should have its world location, this will be used to position chunk in the world terrain. It is
    /// also used as attributes to calculate its height through the Perlin Noise algorithm.
    /// </summary>
    private Vector3 location = new Vector3(0, 0, 0);


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
    /// These are attributes for generating heights for the chunk with Perlin Noise & Fractional Brownian Motion.
    /// </summary>
    private PerlinGrapher grassLayer;
    private PerlinGrapher stoneLayer;
    private PerlinGrapher diamondTopLayer;
    private PerlinGrapher diamondBotLayer;
    private Perlin3DGrapher cave3DLayer;
    private PerlinGrapher caveTopLayer;

    public ChunkMeshBuilder() {

    }

    public ChunkMeshBuilder setLocation(int x, int y, int z) {
        location = new Vector3(x, y, z);
        return this;
    }

    public ChunkMeshBuilder setLocation(Vector3 location) {
        this.location = location;
        return this;
    }

    public ChunkMeshBuilder setDimensions(int width, int height, int depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
        return this;
    }

    public ChunkMeshBuilder setDimensions(Vector3 dimensions) {
        this.width = (int)dimensions.x;
        this.height = (int)dimensions.y;
        this.depth = (int)dimensions.z;
        return this;
    }
    
    public ChunkMeshBuilder setGrassLayerAttribs(PerlinGrapher layer) {
        grassLayer = layer;
        return this;
    }

    public ChunkMeshBuilder setStoneLayerAttribs(PerlinGrapher layer) {
        stoneLayer = layer;
        return this;
    }

    public ChunkMeshBuilder setDiamondLayersAttribs(PerlinGrapher topLayer, PerlinGrapher botLayer) {
        diamondTopLayer = topLayer;
        diamondBotLayer = botLayer;
        return this;
    }

    public ChunkMeshBuilder setCaveLayersAttribs(Perlin3DGrapher cave3DLayer, PerlinGrapher caveTopLayer) {
        this.cave3DLayer = cave3DLayer;
        this.caveTopLayer = caveTopLayer;
        return this;
    }
    
    /// <summary>
    ///     This is the essential function to do the landscaping. It configures all blocks' data in chunk.
    /// Generating heights of the chunk through the use of Perlin Noise with Fractional Brownian Motion
    /// is also configured here.
    ///
    ///     Generated chunk's heights will follow along the curve of the calculated Perlin Noise wave.
    /// </summary>
    private void buildChunk() {
        var blockCount = Width * Depth * Height;
        BlocksTypes = new MeshUtils.BlockType[blockCount];
        for (int i = 0; i < blockCount; i++) {
            // Chunk's world location should be considered to calculate its height in the world using the Perlin Noise algorithm.
            int x = i % Width + (int)location.x;
            int y = (i / Width) % Height + (int)location.y;
            int z = i / (Width * Height) + (int)location.z;

            int grassLayerHeight = (int) MeshUtils.fBM(x, z, grassLayer.Octaves, grassLayer.Scale, grassLayer.HeightScale, grassLayer.HeightOffset);
            int stoneLayerHeight = (int) MeshUtils.fBM(x, z, stoneLayer.Octaves, stoneLayer.Scale, stoneLayer.HeightScale, stoneLayer.HeightOffset);
            int diamondTopLayerHeight = (int) MeshUtils.fBM(x, z, diamondTopLayer.Octaves, diamondTopLayer.Scale,
                diamondTopLayer.HeightScale, diamondTopLayer.HeightOffset);
            int diamondBotLayerHeight = (int) MeshUtils.fBM(x, z, diamondBotLayer.Octaves, diamondBotLayer.Scale,
                diamondBotLayer.HeightScale, diamondBotLayer.HeightOffset);

            int digCave = (int)MeshUtils.fBM3D(x, y, z, cave3DLayer.Octaves, cave3DLayer.Scale, cave3DLayer.HeightScale,
                cave3DLayer.HeightOffset);
            int caveTopLayerHeight = (int)MeshUtils.fBM(x, z, caveTopLayer.Octaves, caveTopLayer.Scale, caveTopLayer.HeightScale,
                caveTopLayer.HeightOffset);

            // Debug.Log("Grass Layer Height: " + grassLayerHeight);
            // Debug.Log("Stone Layer Height: " + stoneLayerHeight);
            
            /*
             *  If the calculated height of the Perlin Noise wave is equal to the height of the grass layer height, it means the block will be the heighest/surface block.
             * Then, make it surface blocks (such as grass).
             *
             * Layers with lower height should have higher privilege in the if/else if statements, or they won't get the chance to spawn blocks.
             */
            if (y == 0) {
                BlocksTypes[i] = MeshUtils.BlockType.BEDROCK;
                /*
                * If the block should be bedrock, there's no need to make decisions further.
                */
                continue;
            }

            if (cave3DLayer != null) {
                if (digCave < cave3DLayer.DrawCutOff && caveTopLayerHeight > y) {
                    BlocksTypes[i] = MeshUtils.BlockType.AIR;
                    continue;
                }
            }
            
            if (grassLayerHeight == y) {
                BlocksTypes[i] = MeshUtils.BlockType.GRASSSIDE;
            } else if (diamondTopLayerHeight > y && diamondBotLayerHeight < y) {
                float random = UnityEngine.Random.Range(0.0f, 1.0f);
                if (random < diamondTopLayer.Probability && random > diamondBotLayer.Probability) {
                    // Debug.Log("Diamond spawned!");
                    BlocksTypes[i] = MeshUtils.BlockType.DIAMOND;
                } else {
                    BlocksTypes[i] = MeshUtils.BlockType.STONE;
                }
            } else if (stoneLayerHeight > y && UnityEngine.Random.Range(0.0f, 1.0f) < stoneLayer.Probability) {
                BlocksTypes[i] = MeshUtils.BlockType.STONE;
            } else if (grassLayerHeight > y) {
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
        BlockMeshBuilder blockMeshBuilder = new BlockMeshBuilder();

        blockMeshes = new Mesh[Width, Height, Depth];

        buildChunk();

        List<Mesh> inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triStart = 0;
        int meshCount = Width * Height * Depth;
        int m = 0;
        /*
        * Building block meshes into chunk is a slow process. To boost up the performance, this project uses Burst compiler and Job system.  
        */
        ProcessMeshDataJob jobs = new ProcessMeshDataJob {
            vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
            triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
        };

        for (int z = 0; z < Depth; z++) {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    MeshUtils.BlockType blockType = BlocksTypes[x + Width * (y + Depth * z)];

                    /*
                     * Block mesh doesn't need to be built if it's air. Make it null instead.
                     * 
                     * Location of the chunk should also be considered as part of block's total offset.
                     */
                    Mesh blockMesh = null;
                    Vector3 blockOffset = new Vector3(x, y, z) + location;
                    switch (blockType) {
                        // Block mesh doesn't need to be built if it's air. Make it null.
                        case MeshUtils.BlockType.AIR:
                            break;
                        case MeshUtils.BlockType.DIRT:
                            blockMesh = blockMeshBuilder.build(this, blockOffset, blockType);
                            break;
                        case MeshUtils.BlockType.GRASSSIDE:
                            // Grass has different top, sides, and bottom.
                            blockMesh = blockMeshBuilder.build(this, blockOffset,
                                MeshUtils.BlockType.GRASSTOP,
                                MeshUtils.BlockType.GRASSSIDE, MeshUtils.BlockType.DIRT);
                            break;
                        case MeshUtils.BlockType.STONE:
                            blockMesh = blockMeshBuilder.build(this, blockOffset,
                                MeshUtils.BlockType.STONE);
                            break;
                        case MeshUtils.BlockType.DIAMOND:
                            blockMesh = blockMeshBuilder.build(this, blockOffset, MeshUtils.BlockType.DIAMOND);
                            break;
                        case MeshUtils.BlockType.BEDROCK:
                            blockMesh = blockMeshBuilder.build(this, blockOffset, MeshUtils.BlockType.BEDROCK);
                            break;
                        default:
                            Debug.LogWarning("This block type: " + blockType + " hasn't been configured yet!");
                            break;
                    }


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
        newMesh.name = "Chunk_" + location.x + '_' + location.y + '_' + location.z;
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
             * These buffer arrays(streams) are configured outside of this struct.
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
    
    public int Width => width;
    public int Height => height;
    public int Depth => depth;

    public Vector3 Location => location;

    public PerlinGrapher GrassLayer => grassLayer;
}