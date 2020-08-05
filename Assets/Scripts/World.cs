﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
	public Material material;
	public BlockType[] blockTypes;
}

[System.Serializable]
public class BlockType {
	public string blockName;
	public bool isSolid;

	public int backFaceTexture;
	public int frontFaceTexture;
	public int topFaceTexture;
	public int bottomFaceTexture;
	public int leftFaceTexture;
	public int rightFaceTexture;

	// Back, Front, Top, Bottom, Left, Right

	public int GetTextureId (int faceIndex) {
		switch (faceIndex) {
			case 0:
				return backFaceTexture;
			case 1:
				return frontFaceTexture;
			case 2:
				return topFaceTexture;
			case 3:
				return bottomFaceTexture;
			case 4:
				return leftFaceTexture;
			case 5:
				return rightFaceTexture;
			default:
				Debug.Log ("Error in GetTextureId, invalid faceIndex");
                // Have a default error texture to use
                return 0;

		}
	}
}
