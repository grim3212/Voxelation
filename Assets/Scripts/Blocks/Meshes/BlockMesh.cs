using UnityEngine;

public class BlockMesh {

    public FaceMeshData[] faces;

}

public class VertData {
	public Vector3 position;
	public Vector2 uv;

	public VertData (Vector3 pos, Vector2 _uv) {
		position = pos;
		uv = _uv;
	}
}
public class FaceMeshData {
	public string direction;
	public Vector3 normal;
	public VertData[] vertData;
	public int[] triangles;
}