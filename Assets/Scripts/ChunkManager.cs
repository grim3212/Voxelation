using System.Collections.Generic;
using UnityEngine;

public class ChunkManager {
    int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<int> transparentTriangles = new List<int> ();
	Material[] materials = new Material[2];
	List<Vector2> uvs = new List<Vector2> ();
	List<Color> colors = new List<Color> ();
	List<Vector3> normals = new List<Vector3> ();

    public ChunkManager () {
        materials[0] = World.Instance.material;
		materials[1] = World.Instance.transparentMaterial;
    }

    private void ClearMeshData() {
        vertexIndex = 0;
		vertices.Clear ();
		triangles.Clear ();
		transparentTriangles.Clear ();
		uvs.Clear ();
		colors.Clear ();
		normals.Clear ();
    }

    

    public MeshData GenerateMeshData(Vector3 pos) {
        return new MeshData();
    }
}

public struct MeshData {

}