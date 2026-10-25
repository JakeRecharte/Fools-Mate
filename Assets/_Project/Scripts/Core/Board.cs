namespace FoolsMate.Core
{
    /// <summary>Board dimensions and coordinate helpers. Square (0, 0) is the bottom-left (a1).</summary>
    public static class Board
    {
        public const int Size = 8;

        public static bool IsOnBoard(int file, int rank)
        {
            return file >= 0 && file < Size && rank >= 0 && rank < Size;
        }
    }
}
