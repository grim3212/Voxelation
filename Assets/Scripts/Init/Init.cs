public class Init {
  public static void Load() {
    LoadBlocks();
  }

  private static void LoadBlocks () {
    BlockRegistry.RegisterBlock(new BlockAir());
    BlockRegistry.RegisterBlock(new BlockGrass());
    BlockRegistry.RegisterBlock(new Block("dirt", 1));
    BlockRegistry.RegisterBlock(new Block("stone", 0));
    BlockRegistry.RegisterBlock(new Block("cobblestone", 8));
    BlockRegistry.RegisterBlock(new Block("bedrock", 9));
    BlockRegistry.RegisterBlock(new Block("sand", 10));
    BlockRegistry.RegisterBlock(new BlockLog());
    BlockRegistry.RegisterBlock(new BlockLeaves());
    BlockRegistry.RegisterBlock(new Block("planks", 4));
    BlockRegistry.RegisterBlock(new Block("bricks", 11));
    BlockRegistry.RegisterBlock(new BlockGlass());
    BlockRegistry.RegisterBlock(new BlockCactus());
  }
}