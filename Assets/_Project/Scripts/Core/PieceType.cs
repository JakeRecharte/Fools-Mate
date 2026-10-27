namespace FoolsMate.Core
{
    public enum PieceType
    {
        King = 1,
        Queen = 2,
        Rook = 3,
        Bishop = 4,
        Knight = 5,
        Pawn = 6,
        None = 0,
    }

    public static class PieceTypes
    {
        public static bool TryFromLetter(char letter, out PieceType type)
        {
            switch (char.ToUpperInvariant(letter))
            {
                case 'K':
                    type = PieceType.King;
                    return true;
                case 'Q':
                    type = PieceType.Queen;
                    return true;
                case 'R':
                    type = PieceType.Rook;
                    return true;
                case 'B':
                    type = PieceType.Bishop;
                    return true;
                case 'N':
                    type = PieceType.Knight;
                    return true;
                case 'P':
                    type = PieceType.Pawn;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }
    }
}
