using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRegistry {
	public readonly static BlockMesh STANDARD = new Standard ();
	public readonly static BlockMesh HALFSLAB = new HalfSlab ();
	private static Dictionary<string, Block> _blocks = new Dictionary<string, Block> ();

	public static Dictionary<string, Block> Blocks {
		get {
			return _blocks;
		}
	}

	public static void RegisterBlock (Block block) {
		_blocks.Add (block.blockId, block);
	}

	public static Block GetBlockById (string blockId) {
		try {
			return _blocks[blockId];
		}
		catch (Exception e) {
			Debug.LogError (e);
			Debug.Log ("Attempted to get non-existant block id: " + blockId);
			return _blocks["air"];
		}
	}
}
