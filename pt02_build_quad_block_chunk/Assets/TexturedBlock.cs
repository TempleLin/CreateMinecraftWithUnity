using UnityEngine;

public class TexturedBlock : MonoBehaviour {
    
    /**
     * Take a look at Quad.cs to understand these fields.
     */
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    /**
     * Material containing Minecraft texture.
     */
    [SerializeField] private Material atlas;

    [SerializeField] private MeshUtils.BlockType _topBlockType;
    [SerializeField] private MeshUtils.BlockType _sideBottomBlockType;
    
    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        _meshRenderer.material = atlas;
        
        QuadBuilder quadBuilder = new QuadBuilder();

        /*
         * Create all quads of a mesh.
         */
        Mesh[] quadsMeshes = {
            /*
            *  Third arg is UV for the block image in texture. Each Minecraft block has UV size of 0.0625 * 0.0625
            * separated evenly.
            */
            quadBuilder.build(BlockSide.TOP, new Vector3(0, 0, 0), _topBlockType),
            quadBuilder.build(BlockSide.BOTTOM, new Vector3(0, 0, 0), _sideBottomBlockType),
            quadBuilder.build(BlockSide.LEFT, new Vector3(0, 0, 0), _sideBottomBlockType),
            quadBuilder.build(BlockSide.RIGHT, new Vector3(0, 0, 0), _sideBottomBlockType),
            quadBuilder.build(BlockSide.FRONT, new Vector3(0, 0, 0), _sideBottomBlockType),
            quadBuilder.build(BlockSide.BACK, new Vector3(0, 0, 0), _sideBottomBlockType),
        };

        /*
         * Merge all the quad meshes into single mesh through this custom utility method.
         */
        _meshFilter.mesh = MeshUtils.mergeMeshes(quadsMeshes);
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}