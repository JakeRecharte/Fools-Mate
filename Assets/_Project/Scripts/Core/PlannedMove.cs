using System;

namespace FoolsMate.Core
{
    /// <summary>A move planned for this turn. From is the piece's cell at the start of the turn.</summary>
    public readonly struct PlannedMove
    {
        public Piece Piece { get; }
        public Cell From { get; }
        public Cell To { get; }

        public PlannedMove(Piece piece, Cell from, Cell to)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (from == to)
            {
                throw new ArgumentException("A move must change cells.", nameof(to));
            }

            Piece = piece;
            From = from;
            To = to;
        }

        public override string ToString() => $"{Piece} {From} → {To}";
    }
}
