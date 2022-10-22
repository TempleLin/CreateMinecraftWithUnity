A voxel is a 3-dimensional equivalent of a pixel.

Voxels don't have geometry, nor do they store coordinate values within themselves.

Just like a pixel is defined by its relative position in an image, a voxel is defined by its relative position inside a volume.

Voxel = Volume + Pixel.

Voxels data is saved in an array:

    int[,,] data = new int[a,b,c];

Making a volume of voxel-per-pixel requires high processing power and memory. When there's not enough, the gap can be filled procedurally,
such as smoothing across the gap algorithmically, or placing a polygon in place.

Voxels can be used to create a highly realistic terrain, or a cube world, depending on the need.
