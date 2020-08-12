using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public class World : MonoBehaviour {

	public Settings settings;
	public BiomeAttributes[] biomes;

	[Range (0f, 1f)]
	public float globalLightLevel;
	public Color day;
	public Color night;

	public Transform player;
	public Vector3 spawnPosition;
	public Material material;
	public Material transparentMaterial;

	Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
	List<Vector2Int> activeChunks = new List<Vector2Int> ();
	public Vector2Int playerChunkCoord;
	Vector2Int playerLastChunkCoord;

	private List<Chunk> chunksToUpdate = new List<Chunk> ();
	public Queue<Chunk> chunksToDraw = new Queue<Chunk> ();

	bool applyingModifications = false;

	Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>> ();

	private bool _inUI = false;

	public Clouds clouds;
	public GameObject debugScreen;

	public GameObject creativeInventory;
	public GameObject cursorSlot;

	Thread ChunkUpdateThread;
	public object ChunkUpdateThreadLock = new object ();
	public object ChunkListThreadLock = new object ();

	private static World _instance;
	public static World Instance { get { return _instance; } }

	public WorldData worldData;

	public string appPath;

	private void Awake () {
		if (_instance != null && _instance != this) {
			Destroy (this.gameObject);
		}
		else {
			_instance = this;
		}

		appPath = Application.persistentDataPath;
	}

	private void Start () {
		// When the world starts load all the blocks
		Init.Load();

		Debug.Log ("Generating new world using seed : " + VoxelData.seed);

		worldData = SaveSystem.LoadWorld ("TestingCaves");

		string jsonImport = File.ReadAllText (Application.dataPath + "/settings.cfg");
		settings = JsonUtility.FromJson<Settings> (jsonImport);

		Random.InitState (VoxelData.seed);

		Shader.SetGlobalFloat ("minLightLevel", VoxelData.minLightLevel);
		Shader.SetGlobalFloat ("maxLightLevel", VoxelData.maxLightLevel);

		LoadWorld ();

		SetGlobalLightValue ();
		spawnPosition = new Vector3 (VoxelData.WorldCenter, VoxelData.ChunkHeight - 50.0f, VoxelData.WorldCenter);
		player.position = spawnPosition;
		CheckViewDistance ();
		playerLastChunkCoord = GetChunkCoordFromVector3 (player.position);

		if (settings.enableThreading) {
			ChunkUpdateThread = new Thread (new ThreadStart (ThreadedUpdate));
			ChunkUpdateThread.Start ();
		}
	}

	public void SetGlobalLightValue () {
		Shader.SetGlobalFloat ("GlobalLightLevel", globalLightLevel);
		Camera.main.backgroundColor = Color.Lerp (night, day, globalLightLevel);
	}

	private void Update () {
		playerChunkCoord = GetChunkCoordFromVector3 (player.position);

		if (!playerChunkCoord.Equals (playerLastChunkCoord)) {
			CheckViewDistance ();
		}

		if (!applyingModifications) {
			ApplyModifications ();
		}

		if (chunksToDraw.Count > 0) {
			chunksToDraw.Dequeue ().CreateMesh ();
		}

		if (!settings.enableThreading) {

			if (!applyingModifications) {
				ApplyModifications ();
			}

			if (chunksToUpdate.Count > 0) {
				UpdateChunks ();
			}
		}

		if (Input.GetKeyDown (KeyCode.F3)) {
			debugScreen.SetActive (!debugScreen.activeSelf);
		}

		if (Input.GetKeyDown (KeyCode.F1)) {
			SaveSystem.SaveWorld (worldData);
		}
	}
	void LoadWorld () {
		for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++) {
			for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++) {
				worldData.LoadChunk (new Vector2Int (x, z));
			}
		}
	}

	public void AddChunkToUpdate (Chunk chunk) {
		AddChunkToUpdate (chunk, false);
	}

	public void AddChunkToUpdate (Chunk chunk, bool insert) {
		lock (ChunkListThreadLock) {

			if (!chunksToUpdate.Contains (chunk)) {
				if (insert)
					chunksToUpdate.Insert (0, chunk);
				else
					chunksToUpdate.Add (chunk);
			}
		}
	}

	void CheckViewDistance () {
		clouds.UpdateClouds ();

		Vector2Int coord = GetChunkCoordFromVector3 (player.position);
		List<Vector2Int> previouslyActiveChunks = new List<Vector2Int> (activeChunks);

		activeChunks.Clear ();

		playerLastChunkCoord = playerChunkCoord;

		for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++) {
			for (int z = coord.y - settings.viewDistance; z < coord.y + settings.viewDistance; z++) {

				Vector2Int thisChunkCoord = new Vector2Int (x, z);

				if (IsChunkInWorld (thisChunkCoord)) {

					if (chunks[x, z] == null)
						chunks[x, z] = new Chunk (thisChunkCoord);

					chunks[x, z].isActive = true;
					activeChunks.Add (thisChunkCoord);
				}

				for (int i = 0; i < previouslyActiveChunks.Count; i++) {
					if (previouslyActiveChunks[i].Equals (thisChunkCoord)) {
						previouslyActiveChunks.RemoveAt (i);
					}
				}

			}
		}

		foreach (Vector2Int c in previouslyActiveChunks) {
			chunks[c.x, c.y].isActive = false;
		}
	}

	void UpdateChunks () {

		lock (ChunkUpdateThreadLock) {
			chunksToUpdate[0].UpdateChunk ();
			if (!activeChunks.Contains (chunksToUpdate[0].coord))
				activeChunks.Add (chunksToUpdate[0].coord);
			chunksToUpdate.RemoveAt (0);
		}
	}

	void ThreadedUpdate () {
		while (true) {
			if (!applyingModifications) {
				ApplyModifications ();
			}

			if (chunksToUpdate.Count > 0) {
				UpdateChunks ();
			}
		}
	}

	private void OnDisable () {
		if (settings.enableThreading)
			ChunkUpdateThread.Abort ();
	}

	void ApplyModifications () {
		applyingModifications = true;

		while (modifications.Count > 0) {

			Queue<VoxelMod> queue = modifications.Dequeue ();

			while (queue.Count > 0) {
				VoxelMod v = queue.Dequeue ();

				worldData.SetVoxel (v.position, v.id);

			}

		}

		applyingModifications = false;
	}

	Vector2Int GetChunkCoordFromVector3 (Vector3 pos) {
		int x = Mathf.FloorToInt (pos.x / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt (pos.z / VoxelData.ChunkWidth);

		return new Vector2Int (x, z);
	}

	public Chunk GetChunkFromVector3 (Vector3 pos) {
		int x = Mathf.FloorToInt (pos.x / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt (pos.z / VoxelData.ChunkWidth);

		return chunks[x, z];
	}


	public bool CheckForVoxel (Vector3 pos) {
		VoxelState voxel = worldData.GetVoxel (pos);
		if (BlockRegistry.GetBlockById(voxel.blockId).isSolid) {
			return true;
		}
		else {
			return false;
		}
	}

	public VoxelState GetVoxelState (Vector3 pos) {
		return worldData.GetVoxel (pos);
	}

	public bool inUI {
		get {
			return _inUI;
		}
		set {
			_inUI = value;
			if (_inUI) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				creativeInventory.SetActive (true);
				cursorSlot.SetActive (true);
			}
			else {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				creativeInventory.SetActive (false);
				cursorSlot.SetActive (false);
			}
		}
	}

	public string GetVoxel (Vector3 pos) {
		int yPos = Mathf.FloorToInt (pos.y);

		// Immutable Pass

		//If outside worlds return air
		if (!IsVoxelInWorld (pos))
			return "air";

		// If bottom block of chunk, return bedrock
		if (yPos == 0)
			return "bedrock";

		// Biome Select Pass
		int solidGroundHeight = 42;
		float sumOfHeights = 0f;
		int count = 0;
		float strongestWeight = 0;
		int strongestBiomeIndex = 0;

		for (int i = 0; i < biomes.Length; i++) {
			float weight = Noise.Get2DPerlin (new Vector2 (pos.x, pos.z), biomes[i].offset, biomes[i].scale);

			// Keep track of which weight is strongest
			if (weight > strongestWeight) {
				strongestWeight = weight;
				strongestBiomeIndex = i;
			}

			// Get heigh of the terrain and multiply it by its weight
			float height = biomes[i].terrainHeight * Noise.Get2DPerlin (new Vector2 (pos.x, pos.z), 0f, biomes[i].terrainScale) * weight;

			// If the height value is greater than 0 add it to the sum of heights
			if (height > 0) {
				sumOfHeights += height;
				count++;
			}
		}

		BiomeAttributes biome = biomes[strongestBiomeIndex];
		// Get the average of the heights

		sumOfHeights /= count;
		int terrainHeight = Mathf.FloorToInt (sumOfHeights + solidGroundHeight);

		// Basic Terrain Pass

		string voxelValue = "air";

		if (yPos == terrainHeight)
			voxelValue = biome.surfaceBlock;
		else if (yPos < terrainHeight && yPos > terrainHeight - 4)
			voxelValue = biome.subSurfaceBlock;
		else if (yPos > terrainHeight)
			return "air";
		else
			voxelValue = "stone";


		// Second Pass

		if (voxelValue == "stone") {
			foreach (Lode lode in biome.lodes) {
				if (yPos > lode.minHeight && yPos < lode.maxHeight) {
					if (Noise.Get3DPerlin (pos, lode.noiseOffset, lode.scale, lode.threshold)) {
						voxelValue = lode.blockId;
					}
				}
			}
		}

		// Tree Pass

		if (yPos == terrainHeight && biome.placeMajorFlora) {

			if (Noise.Get2DPerlin (new Vector2 (pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold) {
				if (Noise.Get2DPerlin (new Vector2 (pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold) {
					modifications.Enqueue (Structure.GenerateMajorFlora (biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
				}
			}

		}

		return voxelValue;
	}

	bool IsChunkInWorld (Vector2Int coord) {
		if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.y > 0 && coord.y < VoxelData.WorldSizeInChunks - 1)
			return true;
		return false;
	}

	bool IsVoxelInWorld (Vector3 pos) {
		if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
			return true;
		return false;
	}
}

public class VoxelMod {
	public Vector3 position;
	public string id;

	public VoxelMod () {
		position = new Vector3 ();
		id = "air";
	}

	public VoxelMod (Vector3 _position, string _id) {
		position = _position;
		id = _id;
	}
}

[System.Serializable]
public class Settings {
	[Header ("Game Data")]
	public string version = "0.0.1";

	[Header ("Performance")]
	public int loadDistance = 16;
	public int viewDistance = 8;
	public bool enableThreading = true;
	public CloudStyle clouds = CloudStyle.Fast;
	public bool enableAnimatedChunks = false;

	[Header ("Controls")]
	[Range (0.1f, 10f)]
	public float mouseSensitivity = 1.75f;

}
