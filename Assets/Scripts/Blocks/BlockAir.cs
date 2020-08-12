public class BlockAir : Block {

  public BlockAir()
  : base("air") {
    this.opacity = 0;
    this.isSolid = false;
    this.renderNeighborFaces = true;
  }

}