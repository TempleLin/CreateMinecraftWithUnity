In-between different heights of a chunk, there should be different block types. Such as grass on top; dirt below grass and higher than certain range; rocks below grass and higher than certain range.

To do that, there should be multiple perlin graphs (layers), each for making decisions on which types of blocks are to be generated between them.

There's also a "Probability" property added to Perlin graphs, to make each block in the defined layer have chance to be generated as the defined block
or other types of block. The decision making statements are located at ChunkMeshBuilder.buildChunk().

Observe the GrassLayer perlin graph, the generated world's chunks' height will follow along the curve of the GrassLayer Perlin graph.

Blocks to draw for different layers are determined in ChunkMeshBuilder.buildChunk().