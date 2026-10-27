namespace FoolsMate.Core
{
    /// <summary>
    /// Board dimensions and coordinate helpers. Cell (0, 0) is always a1 in the game logic; it only
    /// appears at the bottom-left of the screen when the player is white.
    /// </summary>
    public static class Board
    {
        public const int Size = 8;

        public static bool IsOnBoard(int file, int rank)
        {
            return file >= 0 && file < Size && rank >= 0 && rank < Size;
        }

        /// <summary>
        /// Converts a cell seen from one side (rank 0 is that side's back rank) to the real cell.
        /// Only the rank is flipped for black; files are kept so both queens start on the d-file.
        /// </summary>
        public static Cell FromSideView(Cell sideCell, PieceColor side)
        {
            if (side == PieceColor.White)
            {
                return sideCell;
            }
            else
            {
                return new Cell(sideCell.File, Size - 1 - sideCell.Rank);
            }
        }
    }
}
