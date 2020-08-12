public class BlockGlass : Block {

  public BlockGlass()
  : base("glass", 3) {
    this.opacity = 0;
    this.isSolid = true;
    this.renderNeighborFaces = true;
  }

}