using System;
using System.Collections;
using System.Collections.Generic;

public class BlockRegistry {
  public readonly static BlockMesh STANDARD = new Standard();
  public readonly static BlockMesh HALFSLAB = new HalfSlab();
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
		return _blocks[blockId];
	}
}
