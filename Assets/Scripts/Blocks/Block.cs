using UnityEngine;

public class Block {

  public string blockId;
	public BlockMesh mesh;
	public bool isSolid;
	public bool renderNeighborFaces;
	public byte opacity;
	public Sprite icon;


  // The texture id on the texture atlas for each face
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

  public Block(string blockId) {
    this.blockId = blockId;
    this.icon = null;
    this.mesh = BlockRegistry.STANDARD;
    this.opacity = 15;
    this.isSolid = true;
    this.renderNeighborFaces = false;
    this.backFaceTexture = 255;
    this.frontFaceTexture = 255;
    this.topFaceTexture = 255;
    this.bottomFaceTexture = 255;
    this.leftFaceTexture = 255;
    this.rightFaceTexture = 255;
  }

  public Block(string blockId, int texture)
  : this(blockId) {
    this.backFaceTexture = texture;
    this.frontFaceTexture = texture;
    this.topFaceTexture = texture;
    this.bottomFaceTexture = texture;
    this.leftFaceTexture = texture;
    this.rightFaceTexture = texture;
  }

}