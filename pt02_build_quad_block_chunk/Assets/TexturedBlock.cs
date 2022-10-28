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

        BlockBuilder blockBuilder = new BlockBuilder();
        Mesh mesh = blockBuilder.build(new Vector3(0, 0, 0), _topBlockType, _sideBottomBlockType);

        _meshFilter.mesh = mesh;
        _meshFilter.mesh.name = "Cube_0_0_0";
    }
}