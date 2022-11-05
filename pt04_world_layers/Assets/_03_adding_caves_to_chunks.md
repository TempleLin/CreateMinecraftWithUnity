3D Perlin Noise with Fractional Brownian Motion is used to generate caves into chunks.

fBM3D() method is added to MeshUtils class, to be called and used to generate 3D Perlin Noise.

Scene: "_04_3D_perlin_visualization" is for visualizing the use of 3D Perlin Noise onto blocks to generate caves.

Prefab "CavePerlin3D" is used as both visualizing the 3D Perlin Noise effect in Edit mode; and as 3D layer for generating/digging caves in the WorldBuilder.

    To use in Edit mode for visualization, check its "Visualize" checkbox.

    To make it as a layer for use in WorldBuilder, check off the "Visualize" checkbox, and delete all its children gameobjects if there are any.