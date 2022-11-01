using UnityEngine;

/// <summary>
/// Different from PerlinGrapher component, this component not just draws Perlin noise wave lines,
/// it also implements Fractional Brownian Motion.
///
/// Article explaining Fractional Brownian Motion: https://iquilezles.org/articles/fbm/
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class FractionalBrownianMotionPerlinGrapher : MonoBehaviour {
    [SerializeField] private LineRenderer lr;

    /// <summary>
    /// This will adjust the wave noise's amplitude. (this member and scale member might influence each other)
    /// </summary>
    [SerializeField] private float heightScale = 2;
    
    /// <summary>
    /// If given too high x and z value to the Perlin noise height calculation, the result might give height of vertices in-between very small
    /// intervals of the noise, resulting in no visible waviness drawn. This scale member is used for reducing the x and z values.
    /// If given a very small value to this value (~0.14f), the wave turns into smoother curves. 
    /// </summary>
    [SerializeField] private float scale = 0.5f;

    [SerializeField] private float octaves = 1;

    [SerializeField] private float heightOffset = 0;
    
    private void Start() {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 100;
        graph();
    }


    /// <summary>
    /// This OnValidate callback re-executes this component's Start() in edit mode, if there are changes made to the component's
    /// members in inspector. (This only works for components with "ExecuteInEditMode" attribute.)
    /// </summary>
    private void OnValidate() {
        graph();
    }

    /// <summary>
    /// Sets all vertex values in the line renderer.
    /// </summary>
    private void graph() {
        // The Start() might not get called during edit mode, instead this method might get called first due to OnValidate callback. 
        if (lr == null) {
            lr = GetComponent<LineRenderer>();
            lr.positionCount = 100;
        }
        int z = 11;
        Vector3[] positions = new Vector3[lr.positionCount];
        for (int x = 0; x < positions.Length; x++) {
            // float y = Mathf.PerlinNoise(x * scale, z * scale) * heightScale; // Find Perlin noise height value determined by x and z locations.
            float y = fBM(x, z) + heightOffset;
            positions[x] = new Vector3(x, y, z);
        }
        lr.SetPositions(positions);
    }

    private float fBM(float x, float z) {
        float total = 0;
        float frequency = 1;
        for (int i = 0; i < octaves; i++) {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
            frequency *= 2;
        }

        return total;
    }
}