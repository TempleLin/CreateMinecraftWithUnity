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
    
    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        _meshRenderer.material = atlas;
        
        QuadBuilder quadBuilder = new QuadBuilder();

        /*
         *  UV for the block image in texture. Each Minecraft block has UV size of 0.0625 * 0.0625
         * separated evenly.
         */
        Vector2[] atlasUVs = {
            new Vector2(0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f), 
            new Vector2(0.125f, 1.0f), new Vector2(0.1875f, 1.0f)
        };
        
        /*
         * Create all quads of a mesh.
         */
        Mesh[] quadsMeshes = {
            quadBuilder.build(BlockSide.BOTTOM, new Vector3(0, 0, 0), atlasUVs),
            quadBuilder.build(BlockSide.TOP, new Vector3(0, 0, 0), atlasUVs),
            quadBuilder.build(BlockSide.LEFT, new Vector3(0, 0, 0), atlasUVs),
            quadBuilder.build(BlockSide.RIGHT, new Vector3(0, 0, 0), atlasUVs),
            quadBuilder.build(BlockSide.FRONT, new Vector3(0, 0, 0), atlasUVs),
            quadBuilder.build(BlockSide.BACK, new Vector3(0, 0, 0), atlasUVs),
        };

        /*
         * Merge all the quad meshes into single mesh through this custom utility method.
         */
        _meshFilter.mesh = MeshUtils.mergeMeshes(quadsMeshes);
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}