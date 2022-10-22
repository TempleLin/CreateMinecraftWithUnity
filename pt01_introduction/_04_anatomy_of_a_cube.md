A face is a mesh made up of polygons. Each polygon contains two triangles.

A triangle consists of three points, each with coordinates of (x, y, z).

Polygon's normal property tells the engine which side of the triangle should be rendered with lighting and textures. The other side shouldn't be rendered for better performance.

Vertices can also have normals, for use to combine with effects such as bump mapping.

A cube in Unity consists of 12 triangles with edge length of 1.

A cube mesh should contain four arrays:

- Vertex Array (x, y, z coordinates of vertices)
- Normal Array (normal of the corresponding vertex in the vertex array)
- UV Array
- Triangle Array (similar to index buffer in OpenGL)

Unity's component MeshFilter contains the mesh itself. Getting the MeshFilter component of a Unity cube mesh can retrieve these vertices' data in the cube mesh:

    Mesh.mesh.vertices
    Mesh.mesh.normals
    Mesh.mesh.uv
    Mesh.mesh.triangles

Capturing and using those data makes up the very essence of Minecraft-like world.
