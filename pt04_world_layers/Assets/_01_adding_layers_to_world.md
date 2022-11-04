In-between different heights of a chunk, there should be different block types. Such as grass on top; dirt below grass and higher than certain range; rocks below grass and higher than certain range.

To do that, there should be multiple perlin graphs (layers), each for making decisions on which types of blocks are to be generated between them.

Observe the GrassLayer perlin graph, the generated world's chunks' height will follow along the curve of the GrassLayer Perlin graph.

Blocks to draw for different layers are determined in ChunkMeshBuilder.buildChunk().