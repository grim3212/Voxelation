using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelState {
	public byte id;
	[System.NonSerialized]
	private byte _light;

	[System.NonSerialized]
	public ChunkData chunkData;

	[System.NonSerialized]
	public VoxelNeighbors neighbors;

	[System.NonSerialized]
	public Vector3Int position;

	public byte light {
		get {
			return _light;
		}
		set {
			if (value != _light) {

				// Cache the old light and castLight values before updating them
				byte oldLightvalue = _light;
				byte oldCastValue = castLight;

				_light = value;


				// If new light is dark then the old one
				if (_light < oldLightvalue) {

					List<int> neighborsToDarken = new List<int> ();
					for (int p = 0; p < 6; p++) {

						if (neighbors[p] != null) {
							// If a neigbour is less than or equal to our old light value, that means
							// this voxel might have been lighting it up. We want to set it's light value
							// to zero and then it will run its own neighbour checks, but we don't want to
							// do that until we've finished here, so add it to our list and we'll do it
							// after.
							if (neighbors[p].light <= oldCastValue) {
								neighborsToDarken.Add (p);
							}
							else {
								neighbors[p].PropogateLight ();
							}
						}

					}

					foreach (int i in neighborsToDarken) {
						neighbors[i].light = 0;
					}

					if (chunkData.chunk != null) {
						World.Instance.AddChunkToUpdate (chunkData.chunk);
					}

				}
				else if (_light > 1) {
					PropogateLight ();
				}

			}
		}
	}

	public VoxelState (byte _id, ChunkData _chunkData, Vector3Int _position) {
		id = _id;
		chunkData = _chunkData;
		neighbors = new VoxelNeighbors (this);
		position = _position;
		light = 0;
	}

	public Vector3Int globalPosition {
		get {
			return new Vector3Int (position.x + chunkData.position.x, position.y, position.z + chunkData.position.y);
		}
	}

	public float lightAsFloat {
		get {
			return (float)light * VoxelData.unitOfLight;
		}
	}

	public byte castLight {
		get {
			//Get the amount of light this voxel is spreading. Bytes wrap around so we need to do this with an int
			int lightLevel = _light - properties.opacity - 1;
			if (lightLevel < 0) lightLevel = 0;
			return (byte)lightLevel;
		}
	}

	public void PropogateLight () {
		if (light < 2) {
			return;
		}

		for (int p = 0; p < 6; p++) {
			if (neighbors[p] != null) {
				// We can ONLY propogate light in one direction (lighter to darker)
				// If we do it both then we end up with a recursive loop
				if (neighbors[p].light < castLight) {
					neighbors[p].light = castLight;
				}
			}

			if (chunkData.chunk != null) {
				World.Instance.AddChunkToUpdate (chunkData.chunk);
			}
		}
	}

	public BlockType properties {
		get {
			return World.Instance.blockTypes[id];
		}
	}
}

public class VoxelNeighbors {
	public readonly VoxelState parent;
	public VoxelNeighbors (VoxelState _parent) {
		parent = _parent;
	}

	private VoxelState[] _neighbors = new VoxelState[6];
	public int Length { get { return _neighbors.Length; } }

	public VoxelState this[int index] {
		get {
			if (_neighbors[index] == null) {
				_neighbors[index] = World.Instance.worldData.GetVoxel (parent.globalPosition + VoxelData.faceChecks[index]);
				ReturnNeighbor (index);
			}
			return _neighbors[index];
		}
		set {
			_neighbors[index] = value;
			ReturnNeighbor (index);
		}
	}

	void ReturnNeighbor (int index) {
		if (_neighbors[index] == null)
			return;

		if (_neighbors[index].neighbors[VoxelData.reverseFaceCheckIndex[index]] != parent) {
			_neighbors[index].neighbors[VoxelData.reverseFaceCheckIndex[index]] = parent;
		}
	}
}