using UnityEngine;

/// <summary>
///     This component contains algorithm used to generate 3D Perlin noise.
///     This component can be used for visualization of cave generation; and also used as object containing data for
///     generating formal world's caves.
/// </summary>
[ExecuteInEditMode]
public class Perlin3DGrapher : MonoBehaviour {
    [SerializeField] private Vector3 dimensions = new Vector3(10, 10, 10);
    [SerializeField] private float heightScale = 2;
    [SerializeField] [Range(0.0f, 1.0f)] private float scale = 0.5f;
    [SerializeField] private int octaves = 1;
    [SerializeField] private float heightOffset = 1;
    
    /// <summary>
    /// True if is in need of visualizing this 3D Perlin Noise in Edit mode.
    /// </summary>
    [SerializeField] private bool visualize = true;
    
    /// <summary>
    /// This defines the range for cutting off blocks.
    /// </summary>
    [SerializeField] [Range(0.0f, 10.0f)] private float drawCutOff = 1;

    void createCubes()
    {
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "perlin_cube";
                    cube.transform.parent = this.transform;
                    cube.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    void graph()
    {
        //destroy existing cubes
        MeshRenderer[] cubes = GetComponentsInChildren<MeshRenderer>();
        if (cubes.Length == 0)
            createCubes();

        if (cubes.Length == 0) return;

        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    float p3d = MeshUtils.fBM3D(x, y, z, octaves, scale, heightScale, heightOffset);
                    if (p3d < drawCutOff)
                        cubes[x + (int)dimensions.x * (y + (int)dimensions.z * z)].enabled = false;
                    else
                        cubes[x + (int)dimensions.x * (y + (int)dimensions.z * z)].enabled = true;
                }
            }
        }
    }

    void OnValidate()
    {
        if (visualize) {
            graph();
        } 
    }

    public Vector3 Dimensions => dimensions;

    public float HeightScale => heightScale;

    public float Scale => scale;

    public int Octaves => octaves;

    public float HeightOffset => heightOffset;

    public float DrawCutOff => drawCutOff;
}