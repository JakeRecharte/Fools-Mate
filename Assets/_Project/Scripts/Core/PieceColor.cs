namespace FoolsMate.Core
{
    public enum PieceColor
    {
        White,
        Black,
    }

    public static class PieceColors
    {
        public static PieceColor Opposite(this PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return PieceColor.Black;
            }
            else
            {
                return PieceColor.White;
            }
        }
    }
}
