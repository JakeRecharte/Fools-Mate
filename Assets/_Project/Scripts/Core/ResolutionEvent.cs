using System;

namespace FoolsMate.Core
{
    public enum ResolutionEventKind
    {
        /// <summary>The piece reached its planned destination.</summary>
        Moved,

        /// <summary>The piece took Damage and survived.</summary>
        Damaged,

        /// <summary>The piece was taken off the board, and <see cref="ResolutionEvent.By"/> earns its points.</summary>
        Captured,

        /// <summary>The piece was knocked to a random empty cell near where it tried to go.</summary>
        Bounced,

        /// <summary>The piece was taken off the board and nobody earns its points, e.g. a Bounce with no room.</summary>
        Died,
    }

    /// <summary>
    /// One thing that happened while a turn resolved, made by MoveResolver. S2 scoring adds up the Captured
    /// events, and later the view can animate them in order. Colors are stored here because a captured piece is
    /// no longer on the board to ask.
    /// </summary>
    public readonly struct ResolutionEvent
    {
        public ResolutionEventKind Kind { get; }
        public Piece Piece { get; }
        public PieceColor Color { get; }

        /// <summary>Where the piece was before this event.</summary>
        public Cell From { get; }

        /// <summary>Where the piece is after this event. For Captured and Died, the cell it was removed from.</summary>
        public Cell To { get; }

        /// <summary>Damage taken. 0 for every kind except Damaged and Captured.</summary>
        public int Amount { get; }

        /// <summary>The piece responsible, for Damaged and Captured. Null otherwise.</summary>
        public Piece By { get; }

        /// <summary>The color of <see cref="By"/>, i.e. who earns the points. Null when there's no By.</summary>
        public PieceColor? ByColor { get; }

        private ResolutionEvent(ResolutionEventKind kind, Piece piece, PieceColor color, Cell from, Cell to,
            int amount, Piece by, PieceColor? byColor)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            Kind = kind;
            Piece = piece;
            Color = color;
            From = from;
            To = to;
            Amount = amount;
            By = by;
            ByColor = byColor;
        }

        public static ResolutionEvent Moved(Piece piece, PieceColor color, Cell from, Cell to)
        {
            return new ResolutionEvent(ResolutionEventKind.Moved, piece, color, from, to, 0, null, null);
        }

        public static ResolutionEvent Damaged(Piece piece, PieceColor color, Cell cell, int amount, Piece by,
            PieceColor byColor)
        {
            return new ResolutionEvent(ResolutionEventKind.Damaged, piece, color, cell, cell, amount,
                RequireBy(by), byColor);
        }

        public static ResolutionEvent Captured(Piece piece, PieceColor color, Cell cell, int amount, Piece by,
            PieceColor byColor)
        {
            return new ResolutionEvent(ResolutionEventKind.Captured, piece, color, cell, cell, amount,
                RequireBy(by), byColor);
        }

        public static ResolutionEvent Bounced(Piece piece, PieceColor color, Cell from, Cell to)
        {
            return new ResolutionEvent(ResolutionEventKind.Bounced, piece, color, from, to, 0, null, null);
        }

        public static ResolutionEvent Died(Piece piece, PieceColor color, Cell cell)
        {
            return new ResolutionEvent(ResolutionEventKind.Died, piece, color, cell, cell, 0, null, null);
        }

        private static Piece RequireBy(Piece by)
        {
            return by ?? throw new ArgumentNullException(nameof(by));
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case ResolutionEventKind.Moved:
                    return $"{Color} {Piece} moved {From} → {To}";
                case ResolutionEventKind.Damaged:
                    return $"{Color} {Piece} took {Amount} damage from {By} at {To}";
                case ResolutionEventKind.Captured:
                    return $"{Color} {Piece} was captured by {ByColor} {By} at {To}";
                case ResolutionEventKind.Bounced:
                    return $"{Color} {Piece} bounced {From} → {To}";
                default:
                    return $"{Color} {Piece} died at {To}";
            }
        }
    }
}
