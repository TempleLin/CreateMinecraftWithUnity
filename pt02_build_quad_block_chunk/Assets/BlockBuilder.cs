using UnityEngine;

public class BlockBuilder {
    /**
     * Build default block without setting up UV.
     */
    public Mesh build(Vector3 offset) {
        QuadBuilder quadBuilder = new QuadBuilder();
        /*
         * Create all quads of a mesh.
         */
        Mesh[] quadsMeshes = {
            quadBuilder.build(MeshUtils.BlockSide.TOP, offset),
            quadBuilder.build(MeshUtils.BlockSide.BOTTOM, offset),
            quadBuilder.build(MeshUtils.BlockSide.LEFT, offset),
            quadBuilder.build(MeshUtils.BlockSide.RIGHT, offset),
            quadBuilder.build(MeshUtils.BlockSide.FRONT, offset),
            quadBuilder.build(MeshUtils.BlockSide.BACK, offset),
        };

        Mesh mesh = new Mesh();
        /*
        * Merge all the quad meshes into single mesh through this custom utility method.
        */
        mesh = MeshUtils.mergeMeshes(quadsMeshes);
        return mesh;
    }

    public Mesh build(Vector3 offset, MeshUtils.BlockType topBlockType, MeshUtils.BlockType sideBottomBlockType) {
        QuadBuilder quadBuilder = new QuadBuilder();
        Mesh[] quadsMeshes = {
            /*
            *  Third arg is UV for the block image in texture. Each Minecraft block has UV size of 0.0625 * 0.0625
            * separated evenly.
            */
            quadBuilder.build(MeshUtils.BlockSide.TOP, offset, topBlockType),
            quadBuilder.build(MeshUtils.BlockSide.BOTTOM, offset, sideBottomBlockType),
            quadBuilder.build(MeshUtils.BlockSide.LEFT, offset, sideBottomBlockType),
            quadBuilder.build(MeshUtils.BlockSide.RIGHT, offset, sideBottomBlockType),
            quadBuilder.build(MeshUtils.BlockSide.FRONT, offset, sideBottomBlockType),
            quadBuilder.build(MeshUtils.BlockSide.BACK, offset, sideBottomBlockType),
        };

        Mesh mesh = new Mesh();
        mesh = MeshUtils.mergeMeshes(quadsMeshes);
        return mesh;
    }
}