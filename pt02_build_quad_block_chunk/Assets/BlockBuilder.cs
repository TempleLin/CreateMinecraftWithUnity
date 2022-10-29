using System.Collections.Generic;
using UnityEngine;

public class BlockBuilder {
    private readonly QuadBuilder _quadBuilder;

    public BlockBuilder() {
        _quadBuilder = new QuadBuilder();
    }

    /**
     * Build default block without setting up UV.
     */
    public Mesh build(Vector3 offset) {
        /*
         * Create all quads of a mesh.
         */
        return buildMesh(null, offset, MeshUtils.BlockType.AIR, 
            _quadBuilder.build(MeshUtils.BlockSide.TOP, offset),
            _quadBuilder.build(MeshUtils.BlockSide.BOTTOM, offset),
            _quadBuilder.build(MeshUtils.BlockSide.LEFT, offset), 
            _quadBuilder.build(MeshUtils.BlockSide.RIGHT, offset),
            _quadBuilder.build(MeshUtils.BlockSide.FRONT, offset),
            _quadBuilder.build(MeshUtils.BlockSide.BACK, offset));
    }

    public Mesh build(Chunk parentChunk, Vector3 offset, MeshUtils.BlockType blockType) {
        return buildMesh(parentChunk, offset, blockType,
            /*
            *  Third arg is UV for the block image in texture. Each Minecraft block has UV size of 0.0625 * 0.0625
            * separated evenly.
            */
            _quadBuilder.build(MeshUtils.BlockSide.TOP, offset, blockType),
            _quadBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, blockType),
            _quadBuilder.build(MeshUtils.BlockSide.LEFT, offset, blockType),
            _quadBuilder.build(MeshUtils.BlockSide.RIGHT, offset, blockType),
            _quadBuilder.build(MeshUtils.BlockSide.FRONT, offset, blockType),
            _quadBuilder.build(MeshUtils.BlockSide.BACK, offset, blockType));
    }

    /**
     * Some blocks might have different top but others are same. For example, grass.
     */
    public Mesh build(Chunk parentChunk, Vector3 offset, MeshUtils.BlockType topBlockType,
        MeshUtils.BlockType sideBottomBlockType) {
        return buildMesh(parentChunk, offset, topBlockType,
            _quadBuilder.build(MeshUtils.BlockSide.TOP, offset, topBlockType),
            _quadBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, sideBottomBlockType),
            _quadBuilder.build(MeshUtils.BlockSide.LEFT, offset, sideBottomBlockType),
            _quadBuilder.build(MeshUtils.BlockSide.RIGHT, offset, sideBottomBlockType),
            _quadBuilder.build(MeshUtils.BlockSide.FRONT, offset, sideBottomBlockType),
            _quadBuilder.build(MeshUtils.BlockSide.BACK, offset, sideBottomBlockType));
    }

    /// <summary>
    /// Building a final block mesh. This function will make sure that unnecessary faces won't be rendered, by checking
    /// each sides of the neighbours.
    /// </summary>
    /// <param name="parentChunk"></param>
    /// <param name="blockType">Type of the block to build into.</param>
    /// <param name="offset"></param>
    /// <param name="topQuad"></param>
    /// <param name="botQuad"></param>
    /// <param name="leftQuad"></param>
    /// <param name="rightQuad"></param>
    /// <param name="frontQuad"></param>
    /// <param name="backQuad"></param>
    /// <returns></returns>
    private Mesh buildMesh(Chunk parentChunk, Vector3 offset, MeshUtils.BlockType blockType, Mesh topQuad, Mesh botQuad,
        Mesh leftQuad, Mesh rightQuad, Mesh frontQuad, Mesh backQuad) {
        // Only quads that need to be rendered will be added to this list.
        var filteredQuadsMeshes = new List<Mesh>();

        /*
         * Each quad only need to be added if there isn't a neighbour solid block.
         *
         * If the parent chunk is null, just add all the quads to list for process.
         *
         * If the block type is Air, then no need to add any quads to draw.
         */
        if (blockType != MeshUtils.BlockType.AIR) {
            if (parentChunk != null) {
                if (!hasSolidNeighbour(parentChunk, (int)offset.x, (int)offset.y + 1, (int)offset.z))
                    filteredQuadsMeshes.Add(topQuad);
                if (!hasSolidNeighbour(parentChunk, (int)offset.x, (int)offset.y - 1, (int)offset.z))
                    filteredQuadsMeshes.Add(botQuad);
                if (!hasSolidNeighbour(parentChunk, (int)offset.x + 1, (int)offset.y, (int)offset.z))
                    filteredQuadsMeshes.Add(rightQuad);
                if (!hasSolidNeighbour(parentChunk, (int)offset.x - 1, (int)offset.y, (int)offset.z))
                    filteredQuadsMeshes.Add(leftQuad);
                if (!hasSolidNeighbour(parentChunk, (int)offset.x, (int)offset.y, (int)offset.z + 1))
                    filteredQuadsMeshes.Add(frontQuad);
                if (!hasSolidNeighbour(parentChunk, (int)offset.x, (int)offset.y, (int)offset.z - 1))
                    filteredQuadsMeshes.Add(backQuad);
            } else if (parentChunk == null) {
                filteredQuadsMeshes = new List<Mesh> {
                    topQuad,
                    botQuad,
                    leftQuad,
                    rightQuad,
                    frontQuad,
                    backQuad
                };
            }
        }


        Debug.Log("Quads count: " + filteredQuadsMeshes.Count);
        Debug.Log("Filtered quads meshes count: " + filteredQuadsMeshes.Count);
        
        if (filteredQuadsMeshes.Count == 0) return null;
        
        /*
        * Merge all the quad meshes into single mesh through this custom utility method.
        */
        Mesh mesh = MeshUtils.mergeMeshes(filteredQuadsMeshes.ToArray());
        return mesh;
    }

    /// <summary>
    /// For use to judge if the block has neighbours. A block should check for neighbour blocks in 6 sides of direction,
    /// with the help of parent Chunk data. If a side has neighbour block, the face doesn't need to be rendered, thus
    /// saves memory.
    /// </summary>
    /// <param name="parentChunk">Parent chunk containing current block and neighbour blocks.</param>
    /// <param name="x">Neighbour block x offset.</param>
    /// <param name="y">Neighbour block y offset.</param>
    /// <param name="z">Neighbour block z offset.</param>
    private bool hasSolidNeighbour(Chunk parentChunk, int x, int y, int z) {
        // Check if the neighbour block is at the edge of the chunk.
        if (x < 0 || x >= parentChunk.Width || y < 0 || y >= parentChunk.Height || z < 0 ||
            z >= parentChunk.Depth) return false;
        MeshUtils.BlockType blockType = parentChunk.ChunkData[x + parentChunk.Width * (y + parentChunk.Depth * z)];

        // Not just air, if the neighbour is water, the face still needs to be rendered, since water is half transparent.
        if (blockType == MeshUtils.BlockType.AIR || blockType == MeshUtils.BlockType.WATER) return false;

        return true;
    }
}