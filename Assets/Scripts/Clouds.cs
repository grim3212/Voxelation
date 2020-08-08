using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour {
	public int cloudHeight = 100;
	public int cloudDepth = 4;

	[SerializeField]
	private Texture2D cloudPattern = null;
	[SerializeField]
	private Material cloudMaterial = null;
	[SerializeField]
	private World world = null;

	bool[,] cloudData;

	int cloudTexWidth;

	int cloudTileSize;
	Vector3Int offset;

	Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject> ();

	// Start is called before the first frame update
	private void Start () {
		cloudTexWidth = cloudPattern.width;
		cloudTileSize = VoxelData.ChunkWidth;
		offset = new Vector3Int (-(cloudTexWidth / 2), 0, -(cloudTexWidth / 2));

		transform.position = new Vector3 (VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);

		LoadCloudData ();
		CreateClouds ();
	}

	private void LoadCloudData () {
		cloudData = new bool[cloudTexWidth, cloudTexWidth];
		Color[] cloudTex = cloudPattern.GetPixels ();

		for (int x = 0; x < cloudTexWidth; x++) {
			for (int y = 0; y < cloudTexWidth; y++) {
				cloudData[x, y] = (cloudTex[y * cloudTexWidth + x].a > 0);
			}
		}
	}

	private void CreateClouds () {
		if (world.settings.clouds == CloudStyle.Off)
			return;

		for (int x = 0; x < cloudTexWidth; x += cloudTileSize) {
			for (int y = 0; y < cloudTexWidth; y += cloudTileSize) {

				Mesh cloudMesh = world.settings.clouds == CloudStyle.Fast ? CreateFastCloudMesh (x, y) : CreateFancyCloudMesh (x, y);
				Vector3 position = new Vector3 (x, 0, y);
				position += transform.position - new Vector3 (cloudTexWidth / 2f, 0, cloudTexWidth / 2f);
				clouds.Add (CloudTilePosFromVector3 (position), CreateCloudTile (cloudMesh, position));

			}
		}
	}

	public void UpdateClouds () {
		if (world.settings.clouds == CloudStyle.Off)
			return;


		for (int x = 0; x < cloudTexWidth; x += cloudTileSize) {
			for (int y = 0; y < cloudTexWidth; y += cloudTileSize) {
				Vector3 position = world.player.position + new Vector3 (x, 0, y) + offset;
				position = new Vector3 (RoundToCloud (position.x), cloudHeight, RoundToCloud (position.z));
				Vector2Int cloudPosition = CloudTilePosFromVector3 (position);

				clouds[cloudPosition].transform.position = position;
			}
		}
	}

	private int RoundToCloud (float value) {
		return Mathf.FloorToInt (value / cloudTileSize) * cloudTileSize;
	}

	private Mesh CreateFastCloudMesh (int x, int z) {
		List<Vector3> vertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector3> normals = new List<Vector3> ();
		int vertCount = 0;

		for (int xInc = 0; xInc < cloudTileSize; xInc++) {
			for (int zInc = 0; zInc < cloudTileSize; zInc++) {

				int xVal = x + xInc;
				int zVal = z + zInc;

				if (cloudData[xVal, zVal]) {

					//Add four vertices for the cloud face
					vertices.Add (new Vector3 (xInc, 0, zInc));
					vertices.Add (new Vector3 (xInc, 0, zInc + 1));
					vertices.Add (new Vector3 (xInc + 1, 0, zInc + 1));
					vertices.Add (new Vector3 (xInc + 1, 0, zInc));

					// We know what direction our faces are... facing, so we just add them directly
					for (int i = 0; i < 4; i++) {
						normals.Add (Vector3.down);
					}

					// Add first triangle
					triangles.Add (vertCount + 1);
					triangles.Add (vertCount);
					triangles.Add (vertCount + 2);
					// Add second triangle
					triangles.Add (vertCount + 2);
					triangles.Add (vertCount);
					triangles.Add (vertCount + 3);

					// Increment by number of vertices
					vertCount += 4;
				}
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.normals = normals.ToArray ();
		return mesh;
	}

	private Mesh CreateFancyCloudMesh (int x, int z) {
		List<Vector3> vertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector3> normals = new List<Vector3> ();
		int vertCount = 0;

		for (int xInc = 0; xInc < cloudTileSize; xInc++) {
			for (int zInc = 0; zInc < cloudTileSize; zInc++) {

				int xVal = x + xInc;
				int zVal = z + zInc;

				if (cloudData[xVal, zVal]) {

					// Loop through neighbor points using face check array
					for (int p = 0; p < 6; p++) {
						// If the current neighbor has no cloud, draw this face

						if (!CheckCloudData (new Vector3Int (xVal, 0, zVal) + VoxelData.faceChecks[p])) {
							// Add our four vertices for this face

							for (int i = 0; i < 4; i++) {
								Vector3 vert = new Vector3Int (xInc, 0, zInc);
								vert += VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
								vert.y *= cloudDepth;
								vertices.Add (vert);
							}

							for (int i = 0; i < 4; i++) {
								normals.Add (VoxelData.faceChecks[p]);
							}

							triangles.Add (vertCount);
							triangles.Add (vertCount + 1);
							triangles.Add (vertCount + 2);
							triangles.Add (vertCount + 2);
							triangles.Add (vertCount + 1);
							triangles.Add (vertCount + 3);

							vertCount += 4;
						}
					}
				}
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.normals = normals.ToArray ();
		return mesh;
	}

	/*
	*	Return true or false depending on if there is a cloud at the given point
	*/
	private bool CheckCloudData (Vector3Int point) {
		// Because clouds are 2D, if y is above or below 0, return false
		if (point.y != 0) {
			return false;
		}
		int x = point.x;
		int z = point.z;

		if (point.x < 0) x = cloudTexWidth - 1;
		if (point.x > cloudTexWidth - 1) x = 0;
		if (point.z < 0) z = cloudTexWidth - 1;
		if (point.z > cloudTexWidth - 1) z = 0;

		return cloudData[x, z];
	}

	private GameObject CreateCloudTile (Mesh mesh, Vector3 position) {
		GameObject cloudTile = new GameObject ();
		cloudTile.transform.position = position;
		cloudTile.transform.parent = transform;
		cloudTile.name = "Cloud " + position.x + ", " + position.z;
		MeshFilter meshFilter = cloudTile.AddComponent<MeshFilter> ();
		MeshRenderer meshRenderer = cloudTile.AddComponent<MeshRenderer> ();

		meshRenderer.material = cloudMaterial;
		meshFilter.mesh = mesh;

		return cloudTile;
	}

	private Vector2Int CloudTilePosFromVector3 (Vector3 pos) {
		return new Vector2Int (CloudTileCoordFromFloat (pos.x), CloudTileCoordFromFloat (pos.z));
	}

	private int CloudTileCoordFromFloat (float value) {
		//Gets the pos using the cloudTexture as units;
		float a = value / (float)cloudTexWidth;
		a -= Mathf.FloorToInt (a);

		// Multiply cloud texture width by `a` to get the position in the texture globally
		return Mathf.FloorToInt ((float)cloudTexWidth * a);
	}
}

public enum CloudStyle {
	Off,
	Fast,
	Fancy
}
