using UnityEngine;

public class Quad : MonoBehaviour {
    /**
     * For containing the mesh data. (all vertices' data, such as coordinate, normal, UV, triangle etc.)
     */
    private MeshFilter _meshFilter;
    /**
     * For drawing/rendering the mesh itself.
     */
    private MeshRenderer _meshRenderer;

    private void Start() {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        QuadMeshBuilder quadMeshBuilder = new QuadMeshBuilder();
        _meshFilter.mesh = quadMeshBuilder.build();
    }
}