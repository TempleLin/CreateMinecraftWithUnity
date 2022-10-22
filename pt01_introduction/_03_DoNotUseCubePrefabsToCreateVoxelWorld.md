Creating a voxel world cube-by-cube (through generating cube prefabs) is extremely performance-ineffecient. (32x32x32 is already going to get laggy)

Even Minecraft doesn't actually use cube objects to generate the world.

Need to re-invent the algorithms yourself for better performance. (Creating cubes and chunks through voxels. Explained in further tutorials.)
