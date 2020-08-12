using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData {
	public string worldName = "Testing";
	public int seed;

	[System.NonSerialized]
	public Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData> ();

	[System.NonSerialized]
	public List<ChunkData> modifiedChunks = new List<ChunkData> ();

	public void AddToModifiedChunkList (ChunkData chunk) {
		if (!modifiedChunks.Contains (chunk)) {
			modifiedChunks.Add (chunk);
		}
	}

	public WorldData (string _worldName, int _seed) {
		worldName = _worldName;
		seed = _seed;
	}

	public WorldData (WorldData _data) {
		worldName = _data.worldName;
		seed = _data.seed;
	}

	public ChunkData RequestChunk (Vector2Int coord, bool create) {
		ChunkData c;

		if (chunks.ContainsKey (coord)) {
			c = chunks[coord];
		}
		else if (!create) {
			c = null;
		}
		else {
			LoadChunk (coord);
			c = chunks[coord];
		}

		return c;
	}

	public void LoadChunk (Vector2Int coord) {
		if (chunks.ContainsKey (coord)) {
			return;
		}

		ChunkData chunk = SaveSystem.LoadChunk (worldName, coord);

		if (chunk != null) {
			chunks.Add (coord, chunk);
			return;
		}

		chunks.Add (coord, new ChunkData (coord));
		chunks[coord].Populate ();
	}

	bool IsVoxelInWorld (Vector3 pos) {
		if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
			return true;
		return false;
	}

	public void SetVoxel (Vector3 pos, string value) {
		if (!IsVoxelInWorld (pos)) {
			return;
		}

		// Find out the ChunkCoord value of our coxel's chunk
		int x = Mathf.FloorToInt (pos.x / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt (pos.z / VoxelData.ChunkWidth);

		// Then reverse that to get the position of the chunk
		x *= VoxelData.ChunkWidth;
		z *= VoxelData.ChunkWidth;

		// Check if chunk exists, if not create it
		ChunkData chunk = RequestChunk (new Vector2Int (x, z), true);

		// Then create  vector3Int with the position of our voxel within the chunk
		Vector3Int voxel = new Vector3Int ((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

		chunk.ModifyVoxel (voxel, value);
	}

	public VoxelState GetVoxel (Vector3 pos) {
		if (!IsVoxelInWorld (pos)) {
			return null;
		}

		// Find out the ChunkCoord value of our coxel's chunk
		int x = Mathf.FloorToInt (pos.x / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt (pos.z / VoxelData.ChunkWidth);

		// Then reverse that to get the position of the chunk
		x *= VoxelData.ChunkWidth;
		z *= VoxelData.ChunkWidth;

		// Check if chunk exists, if not create it
		ChunkData chunk = RequestChunk (new Vector2Int (x, z), false);

		if (chunk == null) {
			return null;
		}

		// Then create  vector3Int with the position of our voxel within the chunk
		Vector3Int voxel = new Vector3Int ((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

		return chunk.map[voxel.x, voxel.y, voxel.z];
	}
}
