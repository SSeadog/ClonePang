namespace Util
{
    public enum BlockKind
    {
        None,
        BlueBlock,
        GreenBlock,
        PurpleBlock,
        RedBlock,
        YellowBlock,
        DebugBlock
    }

    [System.Serializable]
    public class Pos
    {
        public int y;
        public int x;

        public Pos() { y = 0; x = 0; }
        public Pos(int y, int x) { this.y = y; this.x = x; }
    }
}
