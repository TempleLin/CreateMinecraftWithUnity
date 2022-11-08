using System;
using UnityEngine;

/// <summary>
///     Used for drawing on object, for visualizing noise graph, can also be used to define layers of the world's chunks.
/// Chunks' blocks to generate will be defined by following different layers of Perlin Graphs.
/// 
/// Sometimes this component's start might not run in edit mode. Can force it to run by running the game.
/// </summary>
[ExecuteInEditMode] //This makes it runnable without running game.
[RequireComponent(typeof(LineRenderer))]
public class PerlinGrapher : MonoBehaviour {
    [SerializeField] private LineRenderer lr;

    /// <summary>
    /// This will adjust the wave noise's amplitude. (this member and scale member might influence each other)
    /// </summary>
    [SerializeField] private float heightScale = 10;
    
    /// <summary>
    /// If given too high x and z value to the Perlin noise height calculation, the result might give height of vertices in-between very small
    /// intervals of the noise, resulting in no visible waviness drawn. This scale member is used for reducing the x and z values.
    /// If given a very small value to this value (~0.14f), the wave turns into smoother curves. 
    /// </summary>
    [SerializeField] private float scale = 0.001f;

    [SerializeField] private int octaves = 8;

    [SerializeField] private int heightOffset = 0;

    /// <summary>
    ///     This defines the probability to generate its relevant blocks in the layer. For example, in RockLayer, given a set probability, the blocks
    /// in the range of the layer will generate rocks under given probability.
    /// </summary>
    [SerializeField] private float probability = 0.3f;

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
            float y = MeshUtils.fBM(x, z, octaves, scale, heightScale, heightOffset);
            positions[x] = new Vector3(x, y, z);
        }
        lr.SetPositions(positions);
    }

    public float HeightScale => heightScale;

    public float Scale => scale;

    public int Octaves => octaves;

    public int HeightOffset => heightOffset;

    public float Probability => probability;
}