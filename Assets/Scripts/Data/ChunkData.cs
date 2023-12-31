﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData {
	// The global position of the chunk. ie, (16, 16) NOT (1, 1). We want to be able to
	// access it as a Vector2Int, but Vector2Int's are not serialized so we won't be able
	// to save them. So we'll store them as ints.
	int x;
	int y;

	public Vector2Int position {
		get {
			return new Vector2Int (x, y);
		}
		set {
			x = value.x;
			y = value.y;
		}
	}

	public ChunkData (Vector2Int pos) {
		position = pos;
	}

	[System.NonSerialized]
	public Chunk chunk;

	[HideInInspector]
	[System.NonSerialized]
	public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

	public void Populate () {
		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {
					Vector3 voxelGlobalPos = new Vector3 (x + position.x, y, z + position.y);

					map[x, y, z] = new VoxelState (World.Instance.GetVoxel (voxelGlobalPos), this, new Vector3Int (x, y, z));

					for (int p = 0; p < 6; p++) {
						Vector3Int neighbor = new Vector3Int (x, y, z) + VoxelData.faceChecks[p];
						if (IsVoxelInChunk (neighbor)) {
							map[x, y, z].neighbors[p] = VoxelFromVector3Int (neighbor);
						}
						else {
							map[x, y, z].neighbors[p] = World.Instance.worldData.GetVoxel (voxelGlobalPos + VoxelData.faceChecks[p]);
						}
					}
				}
			}
		}

		Lighting.RecalculateNaturalLight (this);

		World.Instance.worldData.AddToModifiedChunkList (this);
	}

	public void ModifyVoxel (Vector3Int pos, byte _id) {
		if (map[pos.x, pos.y, pos.z].id == _id) {
			return;
		}

		VoxelState voxel = map[pos.x, pos.y, pos.z];
		BlockType newVoxel = World.Instance.blockTypes[_id];

		//Cache old opacity value
		byte oldOpacity = voxel.properties.opacity;

		voxel.id = _id;

		if (voxel.properties.opacity != oldOpacity && (pos.y == VoxelData.ChunkHeight - 1 || map[pos.x, pos.y + 1, pos.z].light == 15)) {
			Lighting.CastNaturalLight (this, pos.x, pos.z, pos.y + 1);
		}

		World.Instance.worldData.AddToModifiedChunkList (this);

		//If chunk attached add for updating
		if (chunk != null) {
			World.Instance.AddChunkToUpdate (chunk);
		}
	}

	public bool IsVoxelInChunk (int x, int y, int z) {
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
			return false;
		return true;
	}

	public bool IsVoxelInChunk (Vector3Int pos) {
		return IsVoxelInChunk (pos.x, pos.y, pos.z);
	}

	public VoxelState VoxelFromVector3Int (Vector3Int pos) {
		return map[pos.x, pos.y, pos.z];
	}

}
