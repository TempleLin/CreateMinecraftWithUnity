﻿using System.Collections.Generic;
using UnityEngine;

public class BlockMeshBuilder {
    private readonly QuadMeshBuilder _quadMeshBuilder;

    public BlockMeshBuilder() {
        _quadMeshBuilder = new QuadMeshBuilder();
    }

    /**
     * Build default block without setting up UV.
     */
    public Mesh build(Vector3 offset) {
        /*
         * Create all quads of a mesh.
         */
        return buildMesh(null, offset, MeshUtils.BlockType.AIR, _quadMeshBuilder.build(MeshUtils.BlockSide.TOP, offset),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BOTTOM, offset),
            _quadMeshBuilder.build(MeshUtils.BlockSide.LEFT, offset), _quadMeshBuilder.build(MeshUtils.BlockSide.RIGHT, offset),
            _quadMeshBuilder.build(MeshUtils.BlockSide.FRONT, offset),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BACK, offset));
    }

    public Mesh build(ChunkMeshBuilder parentChunkMesh, Vector3 offset, MeshUtils.BlockType blockType) {
        return buildMesh(parentChunkMesh, offset, blockType,
            /*
            *  Third arg is UV for the block image in texture. Each Minecraft block has UV size of 0.0625 * 0.0625
            * separated evenly.
            */
            _quadMeshBuilder.build(MeshUtils.BlockSide.TOP, offset, blockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, blockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.LEFT, offset, blockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.RIGHT, offset, blockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.FRONT, offset, blockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BACK, offset, blockType));
    }


    /// <summary>
    /// Some blocks might have different top but other sides are the same.
    /// </summary>
    /// <param name="parentChunkMesh"></param>
    /// <param name="offset"></param>
    /// <param name="topBlockType"></param>
    /// <param name="sideBottomBlockType"></param>
    /// <returns></returns>
    public Mesh build(ChunkMeshBuilder parentChunkMesh, Vector3 offset, MeshUtils.BlockType topBlockType,
        MeshUtils.BlockType sideBottomBlockType) {
        return buildMesh(parentChunkMesh, offset, topBlockType,
            _quadMeshBuilder.build(MeshUtils.BlockSide.TOP, offset, topBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, sideBottomBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.LEFT, offset, sideBottomBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.RIGHT, offset, sideBottomBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.FRONT, offset, sideBottomBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BACK, offset, sideBottomBlockType));
    }

    /// <summary>
    /// Some blocks will have different top, sides, and bottom sides. (ex. grass)
    /// </summary>
    /// <param name="parentChunkMesh"></param>
    /// <param name="offset"></param>
    /// <param name="topBlockType"></param>
    /// <param name="sidesBlockType"></param>
    /// <param name="bottomBlockType"></param>
    /// <returns></returns>
    public Mesh build(ChunkMeshBuilder parentChunkMesh, Vector3 offset, MeshUtils.BlockType topBlockType,
        MeshUtils.BlockType sidesBlockType, MeshUtils.BlockType bottomBlockType) {
        return buildMesh(parentChunkMesh, offset, topBlockType,
            _quadMeshBuilder.build(MeshUtils.BlockSide.TOP, offset, topBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, bottomBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.LEFT, offset, sidesBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.RIGHT, offset, sidesBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.FRONT, offset, sidesBlockType),
            _quadMeshBuilder.build(MeshUtils.BlockSide.BACK, offset, sidesBlockType));
    }

    /// <summary>
    ///     Building a final block mesh. This function will make sure that unnecessary faces won't be rendered, by checking
    ///     each sides of the neighbours.
    /// </summary>
    /// <param name="parentChunkMesh"></param>
    /// <param name="blockType">Type of the block to build into.</param>
    /// <param name="offset"></param>
    /// <param name="topQuad"></param>
    /// <param name="botQuad"></param>
    /// <param name="leftQuad"></param>
    /// <param name="rightQuad"></param>
    /// <param name="frontQuad"></param>
    /// <param name="backQuad"></param>
    /// <returns></returns>
    private Mesh buildMesh(ChunkMeshBuilder parentChunkMesh, Vector3 offset, MeshUtils.BlockType blockType, Mesh topQuad,
        Mesh botQuad, Mesh leftQuad, Mesh rightQuad, Mesh frontQuad, Mesh backQuad) {
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
            if (parentChunkMesh != null) {

                /*
                 *  Passed offset from parent chunk has chunk's world location combined in offset. But calculating solid neighbours should be using
                 * local positions of the blocks in the chunk.
                 */
                Vector3 blockLocalPos = offset - parentChunkMesh.Location;
                
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x, (int)blockLocalPos.y + 1, (int)blockLocalPos.z))
                    filteredQuadsMeshes.Add(topQuad);
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x, (int)blockLocalPos.y - 1, (int)blockLocalPos.z))
                    filteredQuadsMeshes.Add(botQuad);
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x + 1, (int)blockLocalPos.y, (int)blockLocalPos.z))
                    filteredQuadsMeshes.Add(rightQuad);
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x - 1, (int)blockLocalPos.y, (int)blockLocalPos.z))
                    filteredQuadsMeshes.Add(leftQuad);
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x, (int)blockLocalPos.y, (int)blockLocalPos.z + 1))
                    filteredQuadsMeshes.Add(frontQuad);
                if (!hasSolidNeighbour(parentChunkMesh, (int)blockLocalPos.x, (int)blockLocalPos.y, (int)blockLocalPos.z - 1))
                    filteredQuadsMeshes.Add(backQuad);
            } else {
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


        // Debug.Log("Quads count: " + filteredQuadsMeshes.Count);
        // Debug.Log("Filtered quads meshes count: " + filteredQuadsMeshes.Count);

        if (filteredQuadsMeshes.Count == 0) return null;

        /*
        * Merge all the quad meshes into single mesh through this custom utility method.
        */
        var mesh = MeshUtils.mergeMeshes(filteredQuadsMeshes.ToArray());
        return mesh;
    }

    /// <summary>
    ///     For use to judge if the block has neighbours. A block should check for neighbour blocks in 6 sides of direction,
    ///     with the help of parent Chunk data. If a side has neighbour block, the face doesn't need to be rendered, thus
    ///     saves memory.
    /// </summary>
    /// <param name="parentChunkMesh">Parent chunk containing current block and neighbour blocks.</param>
    /// <param name="x">Neighbour block x offset.</param>
    /// <param name="y">Neighbour block y offset.</param>
    /// <param name="z">Neighbour block z offset.</param>
    private bool hasSolidNeighbour(ChunkMeshBuilder parentChunkMesh, int x, int y, int z) {
        // Check if the neighbour block is at the edge of the chunk.
        if (x < 0 || x >= parentChunkMesh.Width || y < 0 || y >= parentChunkMesh.Height || z < 0 ||
            z >= parentChunkMesh.Depth) return false;
        MeshUtils.BlockType blockType = parentChunkMesh.BlocksTypes[x + parentChunkMesh.Width * (y + parentChunkMesh.Depth * z)];

        // Not just air, if the neighbour is water, the face still needs to be rendered, since water is half transparent.
        if (blockType == MeshUtils.BlockType.AIR || blockType == MeshUtils.BlockType.WATER) return false;

        return true;
    }
}