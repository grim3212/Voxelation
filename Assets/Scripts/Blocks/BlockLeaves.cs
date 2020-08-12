public class BlockLeaves : Block {

  public BlockLeaves()
  : base("leaves", 16) {
    this.opacity = 5;
    this.isSolid = true;
    this.renderNeighborFaces = true;
  }

}