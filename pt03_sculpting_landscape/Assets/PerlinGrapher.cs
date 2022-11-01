using System;
using UnityEngine;

/// <summary>
///     Used for drawing on object, for visualizing noise graph.
/// Sometimes this component's start might not run in edit mode. Can force it to run by running the game.
/// </summary>
[ExecuteInEditMode] //This makes it runnable without running game.
[RequireComponent(typeof(LineRenderer))]
public class PerlinGrapher : MonoBehaviour {
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
    private void Start() {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 100;
        graph();
    }


    /// <summary>
    /// This OnValidate callback re-executes this component's start() in edit mode, if there are changes made to the component's
    /// members in inspector. (This only works for components with "ExecuteInEditMode" attribute.)
    /// </summary>
    private void OnValidate() {
        graph();
    }

    /// <summary>
    /// Sets all vertex values in the line renderer.
    /// </summary>
    private void graph() {
        int z = 11;
        Vector3[] positions = new Vector3[lr.positionCount];
        for (int x = 0; x < positions.Length; x++) {
            float y = Mathf.PerlinNoise(x * scale, z * scale) * heightScale; // Find Perlin noise height value determined by x and z locations.  
            positions[x] = new Vector3(x, y, z);
        }
        lr.SetPositions(positions);
    }
}