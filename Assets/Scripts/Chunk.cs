using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

	public Vector2Int coord;
	GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<int> transparentTriangles = new List<int> ();
	Material[] materials = new Material[2];
	List<Vector2> uvs = new List<Vector2> ();
	List<Color> colors = new List<Color> ();
	List<Vector3> normals = new List<Vector3> ();

	public Vector3 position;
	private bool _isActive;
	ChunkData chunkData;

	public Chunk (Vector2Int _coord) {
		coord = _coord;

		chunkObject = new GameObject ();
		meshFilter = chunkObject.AddComponent<MeshFilter> ();
		meshRenderer = chunkObject.AddComponent<MeshRenderer> ();

		materials[0] = World.Instance.material;
		materials[1] = World.Instance.transparentMaterial;
		meshRenderer.materials = materials;

		chunkObject.transform.SetParent (World.Instance.transform);
		chunkObject.transform.position = new Vector3 (coord.x * VoxelData.ChunkWidth, 0.0f, coord.y * VoxelData.ChunkWidth);
		chunkObject.name = "Chunk [" + coord.x + "," + coord.y + "]";
		position = chunkObject.transform.position;

		chunkData = World.Instance.worldData.RequestChunk (new Vector2Int ((int)position.x, (int)position.z), true);
		chunkData.chunk = this;

		World.Instance.AddChunkToUpdate (this);

		if (World.Instance.settings.enableAnimatedChunks) {
			chunkObject.AddComponent<ChunkLoadAnimation> ();
		}
	}

	public void UpdateChunk () {

		ClearMeshData ();

		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {
					if (World.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
						UpdateMeshData (new Vector3 (x, y, z));
				}
			}
		}

		lock (World.Instance.chunksToDraw) {
			World.Instance.chunksToDraw.Enqueue (this);
		}

	}



	void ClearMeshData () {
		vertexIndex = 0;
		vertices.Clear ();
		triangles.Clear ();
		transparentTriangles.Clear ();
		uvs.Clear ();
		colors.Clear ();
		normals.Clear ();
	}

	void UpdateMeshData (Vector3 pos) {
		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		VoxelState voxel = chunkData.map[x, y, z];

		for (int p = 0; p < 6; p++) {

			VoxelState neighbor = chunkData.map[x, y, z].neighbors[p];

			if (neighbor != null && neighbor.properties.renderNeighborFaces) {

				float lightLevel = neighbor.lightAsFloat;
				int faceVertCount = 0;

				for (int i = 0; i < voxel.properties.voxelMesh.faces[p].vertData.Length; i++) {
					vertices.Add (pos + voxel.properties.voxelMesh.faces[p].vertData[i].position);
					normals.Add (voxel.properties.voxelMesh.faces[p].normal);
					colors.Add (new Color (0, 0, 0, lightLevel));
					AddTexture(voxel.properties.GetTextureId(p), voxel.properties.voxelMesh.faces[p].vertData[i].uv);
					faceVertCount++;
				}

				if (!voxel.properties.renderNeighborFaces) {
					for (int i = 0; i < voxel.properties.voxelMesh.faces[p].triangles.Length; i++) {
						triangles.Add (vertexIndex + voxel.properties.voxelMesh.faces[p].triangles[i]);
					}
				}
				else {
					for (int i = 0; i < voxel.properties.voxelMesh.faces[p].triangles.Length; i++) {
						transparentTriangles.Add (vertexIndex + voxel.properties.voxelMesh.faces[p].triangles[i]);
					}
				}

				vertexIndex += faceVertCount;
			}
		}
	}

	public bool isActive {
		get {
			return _isActive;
		}
		set {
			_isActive = value;
			if (chunkObject != null) {
				chunkObject.SetActive (value);
			}
		}
	}

	public void EditVoxel (Vector3 pos, byte newId) {
		int xCheck = Mathf.FloorToInt (pos.x);
		int yCheck = Mathf.FloorToInt (pos.y);
		int zCheck = Mathf.FloorToInt (pos.z);

		xCheck -= Mathf.FloorToInt (chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt (chunkObject.transform.position.z);

		chunkData.ModifyVoxel (new Vector3Int (xCheck, yCheck, zCheck), newId);

		// Update surround chunks
		UpdateSurroundVoxels (xCheck, yCheck, zCheck);

	}

	void UpdateSurroundVoxels (int x, int y, int z) {
		Vector3 thisVoxel = new Vector3 (x, y, z);

		for (int p = 0; p < 6; p++) {
			Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

			if (!chunkData.IsVoxelInChunk ((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z)) {
				World.Instance.AddChunkToUpdate (World.Instance.GetChunkFromVector3 (currentVoxel + position), true);
			}
		}
	}

	public VoxelState GetVoxelFromGlobalVector3 (Vector3 pos) {
		int xCheck = Mathf.FloorToInt (pos.x);
		int yCheck = Mathf.FloorToInt (pos.y);
		int zCheck = Mathf.FloorToInt (pos.z);

		xCheck -= Mathf.FloorToInt (position.x);
		zCheck -= Mathf.FloorToInt (position.z);

		return chunkData.map[xCheck, yCheck, zCheck];
	}

	public void CreateMesh () {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();

		mesh.subMeshCount = 2;
		mesh.SetTriangles (triangles.ToArray (), 0);
		mesh.SetTriangles (transparentTriangles.ToArray (), 1);

		mesh.uv = uvs.ToArray ();
		mesh.colors = colors.ToArray ();

		mesh.normals = normals.ToArray ();

		meshFilter.mesh = mesh;
	}

	void AddTexture (int textureId, Vector2 uv) {
		float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
		float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

		x *= VoxelData.NormalizedBlockTextureSize;
		y *= VoxelData.NormalizedBlockTextureSize;

		y = 1.0f - y - VoxelData.NormalizedBlockTextureSize;

		x += VoxelData.NormalizedBlockTextureSize * uv.x;
		y += VoxelData.NormalizedBlockTextureSize * uv.y;

		uvs.Add (new Vector2 (x, y));
	}
}
