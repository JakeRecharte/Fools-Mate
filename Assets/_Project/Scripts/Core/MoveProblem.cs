namespace FoolsMate.Core
{
    public enum MoveProblemKind
    {
        /// <summary>One of your own pieces is on the destination and isn't moving this turn.</summary>
        DestinationOccupied,

        /// <summary>The destination is no longer a legal target, e.g. a path blocked after the plan changed.</summary>
        Unreachable,

        /// <summary>Reserved for the check rule, which arrives with the enemy phase.</summary>
        KingInCheck,
    }

    /// <summary>One reason the plan can't go ahead, found by <see cref="MoveCheck"/>.</summary>
    public readonly struct MoveProblem
    {
        public Piece Piece { get; }
        public MoveProblemKind Kind { get; }
        public string Message { get; }

        public MoveProblem(Piece piece, MoveProblemKind kind, string message)
        {
            Piece = piece;
            Kind = kind;
            Message = message;
        }

        public override string ToString() => Message;
    }
}
