using UnityEngine;

public class Standard : BlockMesh {

	public Standard () {
		this.faces = new FaceMeshData[6];

		// Back face
		this.faces[0] = new FaceMeshData ();
		this.faces[0].direction = "Back";
		this.faces[0].normal = new Vector3 (0, 0, -1);
		this.faces[0].vertData = new VertData[4];
		this.faces[0].vertData[0] = new VertData (new Vector3 (0, 0, 0), new Vector2 (0, 0));
		this.faces[0].vertData[1] = new VertData (new Vector3 (0, 1, 0), new Vector2 (0, 1));
		this.faces[0].vertData[2] = new VertData (new Vector3 (1, 1, 0), new Vector2 (1, 1));
		this.faces[0].vertData[3] = new VertData (new Vector3 (1, 0, 0), new Vector2 (1, 0));
		this.faces[0].triangles = new int[] { 0, 1, 3, 3, 1, 2 };

		// Front face
		this.faces[1] = new FaceMeshData ();
		this.faces[1].direction = "Front";
		this.faces[1].normal = new Vector3 (0, 0, 1);
		this.faces[1].vertData = new VertData[4];
		this.faces[1].vertData[0] = new VertData (new Vector3 (0, 0, 1), new Vector2 (0, 0));
		this.faces[1].vertData[1] = new VertData (new Vector3 (0, 1, 1), new Vector2 (0, 1));
		this.faces[1].vertData[2] = new VertData (new Vector3 (1, 1, 1), new Vector2 (1, 1));
		this.faces[1].vertData[3] = new VertData (new Vector3 (1, 0, 1), new Vector2 (1, 0));
		this.faces[1].triangles = new int[] { 0, 3, 1, 1, 3, 2 };


		// Top face
		this.faces[2] = new FaceMeshData ();
		this.faces[2].direction = "Top";
		this.faces[2].normal = new Vector3 (0, 1, 1);
		this.faces[2].vertData = new VertData[4];
		this.faces[2].vertData[0] = new VertData (new Vector3 (0, 1, 0), new Vector2 (0, 0));
		this.faces[2].vertData[1] = new VertData (new Vector3 (0, 1, 1), new Vector2 (0, 1));
		this.faces[2].vertData[2] = new VertData (new Vector3 (1, 1, 1), new Vector2 (1, 1));
		this.faces[2].vertData[3] = new VertData (new Vector3 (1, 1, 0), new Vector2 (1, 0));
		this.faces[2].triangles = new int[] { 0, 1, 3, 3, 1, 2 };


		// Bottom face
		this.faces[3] = new FaceMeshData ();
		this.faces[3].direction = "Bottom";
		this.faces[3].normal = new Vector3 (0, -1, 0);
		this.faces[3].vertData = new VertData[4];
		this.faces[3].vertData[0] = new VertData (new Vector3 (0, 0, 0), new Vector2 (0, 0));
		this.faces[3].vertData[1] = new VertData (new Vector3 (0, 0, 1), new Vector2 (0, 1));
		this.faces[3].vertData[2] = new VertData (new Vector3 (1, 0, 1), new Vector2 (1, 1));
		this.faces[3].vertData[3] = new VertData (new Vector3 (1, 0, 0), new Vector2 (1, 0));
		this.faces[3].triangles = new int[] { 0, 3, 1, 1, 3, 2 };


		// Left face
		this.faces[4] = new FaceMeshData ();
		this.faces[4].direction = "Left";
		this.faces[4].normal = new Vector3 (-1, 0, 0);
		this.faces[4].vertData = new VertData[4];
		this.faces[4].vertData[0] = new VertData (new Vector3 (0, 0, 0), new Vector2 (0, 0));
		this.faces[4].vertData[1] = new VertData (new Vector3 (0, 1, 0), new Vector2 (0, 1));
		this.faces[4].vertData[2] = new VertData (new Vector3 (0, 1, 1), new Vector2 (1, 1));
		this.faces[4].vertData[3] = new VertData (new Vector3 (0, 0, 1), new Vector2 (1, 0));
		this.faces[4].triangles = new int[] { 0, 3, 1, 1, 3, 2 };


		// Right face
		this.faces[5] = new FaceMeshData ();
		this.faces[5].direction = "Right";
		this.faces[5].normal = new Vector3 (1, 0, 0);
		this.faces[5].vertData = new VertData[4];
		this.faces[5].vertData[0] = new VertData (new Vector3 (1, 0, 0), new Vector2 (0, 0));
		this.faces[5].vertData[1] = new VertData (new Vector3 (1, 1, 0), new Vector2 (0, 1));
		this.faces[5].vertData[2] = new VertData (new Vector3 (1, 1, 1), new Vector2 (1, 1));
		this.faces[5].vertData[3] = new VertData (new Vector3 (1, 0, 1), new Vector2 (1, 0));
		this.faces[5].triangles = new int[] { 0, 1, 3, 3, 1, 2 };
	}

}