using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    /// <summary>
    /// This turn's planned moves, one per piece, limited by energy. It doesn't check whether moves are legal:
    /// <see cref="MoveTargets"/> decides what can be planned, and the Move Check validates the whole plan
    /// when MOVE is pressed.
    /// </summary>
    public class TurnPlan
    {
        private readonly Dictionary<Piece, PlannedMove> moves = new Dictionary<Piece, PlannedMove>();

        public TurnPlan(int energy)
        {
            if (energy < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(energy), "Energy can't be negative.");
            }

            Energy = energy;
        }

        public int Energy { get; }
        public int EnergyUsed => moves.Count;
        public int EnergyLeft => Energy - EnergyUsed;

        public IEnumerable<PlannedMove> Moves => moves.Values;

        /// <summary>Pieces with a planned move. Pass to <see cref="MoveTargets.GetTargets"/> for pass-through.</summary>
        public ICollection<Piece> MovingPieces => moves.Keys;

        /// <summary>
        /// Plans a move, replacing the piece's existing move for free. Returns false if the piece has no move
        /// yet and there's no energy left.
        /// </summary>
        public bool TryPlan(PlannedMove move)
        {
            if (move.Piece == null)
            {
                throw new ArgumentException("The move has no piece.", nameof(move));
            }

            if (!moves.ContainsKey(move.Piece) && EnergyLeft <= 0)
            {
                return false;
            }

            moves[move.Piece] = move;
            return true;
        }

        /// <summary>Removes the piece's planned move, refunding its energy. Returns false if it had none.</summary>
        public bool Cancel(Piece piece)
        {
            return piece != null && moves.Remove(piece);
        }

        public bool TryGetMove(Piece piece, out PlannedMove move)
        {
            if (piece == null)
            {
                move = default;
                return false;
            }

            return moves.TryGetValue(piece, out move);
        }

        public void Clear()
        {
            moves.Clear();
        }
    }
}
