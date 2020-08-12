using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem {
	public static void SaveWorld (WorldData world) {
		// set save location
		string savePath = World.Instance.appPath + "/saves/" + world.worldName + "/";

		if (!Directory.Exists (savePath)) {
			Directory.CreateDirectory (savePath);
		}

		Debug.Log ("Saving " + world.worldName);

		BinaryFormatter formatter = new BinaryFormatter ();
		FileStream stream = new FileStream (savePath + "world.world", FileMode.Create);

		formatter.Serialize (stream, world);
		stream.Close ();

		Thread thread = new Thread (() => SaveChunks (world));
		thread.Start ();
	}

	public static void SaveChunks (WorldData world) {
		List<ChunkData> chunks = new List<ChunkData> (world.modifiedChunks);
		world.modifiedChunks.Clear ();
		LoadCurrentBlockMap ();

		int count = 0;
		foreach (ChunkData chunk in chunks) {
			SaveSystem.SaveChunk (chunk, world.worldName);
			count++;
		}
		Debug.Log ("Saved " + count + " chunks.");
	}

	public static WorldData LoadWorld (string worldName, int seed = 0) {
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";

		if (File.Exists (loadPath + "world.world")) {
			Debug.Log ("Loading world for save " + worldName);

			BinaryFormatter formatter = new BinaryFormatter ();
			FileStream stream = new FileStream (loadPath + "world.world", FileMode.Open);

			WorldData world = formatter.Deserialize (stream) as WorldData;
			stream.Close ();
			return new WorldData (world);
		}
		else {
			Debug.Log (worldName + " not found. Generating new world");

			WorldData world = new WorldData (worldName, seed);
			SaveWorld (world);

			return world;
		}
	}

	private static Dictionary<string, int> _currentBlockMap;

	private static void LoadCurrentBlockMap () {
		_currentBlockMap = new Dictionary<string, int> ();
		int idx = 0;
		foreach (string blockId in BlockRegistry.Blocks.Keys) {
			_currentBlockMap.Add (blockId, idx);
			idx++;
		}
	}

	public static void SaveChunk (ChunkData chunk, string worldName) {
		string chunkName = chunk.position.x + "-" + chunk.position.y;
		// set save location
		string savePath = World.Instance.appPath + "/saves/" + worldName + "/chunks/";

		if (!Directory.Exists (savePath)) {
			Directory.CreateDirectory (savePath);
		}

		using (FileStream stream = new FileStream (savePath + chunkName + ".chunk", FileMode.Create)) {
			using (BinaryWriter bw = new BinaryWriter (stream)) {
				bw.Write (chunk.position.x);
				bw.Write (chunk.position.y);

				// We need the count to be abel to read this back
				bw.Write (_currentBlockMap.Count);
				// Write the mapping from string to id that we are using for this chunk
				foreach (KeyValuePair<string, int> map in _currentBlockMap) {
					bw.Write (map.Key);
					bw.Write (map.Value);
				}

				int[] voxels = new int[VoxelData.ChunkWidth * VoxelData.ChunkHeight * VoxelData.ChunkWidth];
				byte[] lights = new byte[VoxelData.ChunkWidth * VoxelData.ChunkHeight * VoxelData.ChunkWidth];
				for (int x = 0; x < VoxelData.ChunkWidth; x++) {
					for (int y = 0; y < VoxelData.ChunkHeight; y++) {
						for (int z = 0; z < VoxelData.ChunkWidth; z++) {

							voxels[x + VoxelData.ChunkWidth * (y + VoxelData.ChunkHeight * z)] = _currentBlockMap[chunk.map[x, y, z].blockId];
							lights[x + VoxelData.ChunkWidth * (y + VoxelData.ChunkHeight * z)] = chunk.map[x, y, z].light;
						}
					}
				}

				bw.Write (lights);
				for (int i = 0; i < voxels.Length; i++) {
					bw.Write (voxels[i]);
				}


				bw.Flush ();
			}
		}
	}

	public static ChunkData LoadChunk (string worldName, Vector2Int position) {
		string chunkName = position.x + "-" + position.y;
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName + ".chunk";

		if (File.Exists (loadPath)) {
			ChunkData chunkData = null;
			using (FileStream stream = new FileStream (loadPath, FileMode.Open)) {
				using (BinaryReader br = new BinaryReader (stream)) {
					int count = VoxelData.ChunkWidth * VoxelData.ChunkHeight * VoxelData.ChunkWidth;
					int posX = br.ReadInt32 ();
					int posY = br.ReadInt32 ();

					int blockMapCount = br.ReadInt32 ();

					Dictionary<int, string> chunkBlockMap = new Dictionary<int, string> ();
					for (int i = 0; i < blockMapCount; i++) {
						string blockId = br.ReadString ();
						int mappedBlockId = br.ReadInt32 ();
						chunkBlockMap.Add (mappedBlockId, blockId);
					}

					byte[] lights = br.ReadBytes (count);

					int[] voxels = new int[count];
					for (int i = 0; i < count; i++) {
						voxels[i] = br.ReadInt32 ();
					}

					chunkData = new ChunkData (new Vector2Int (posX, posY));

					for (int x = 0; x < VoxelData.ChunkWidth; x++) {
						for (int y = 0; y < VoxelData.ChunkHeight; y++) {
							for (int z = 0; z < VoxelData.ChunkWidth; z++) {
								int index = x + VoxelData.ChunkWidth * (y + VoxelData.ChunkHeight * z);
								VoxelState state = new VoxelState (chunkBlockMap[voxels[index]], chunkData, new Vector3Int (x, y, z));
								state.light = lights[index];
								chunkData.map[x, y, z] = state;
							}
						}
					}
				}
			}
			return chunkData;
		}
		return null;
	}
}
