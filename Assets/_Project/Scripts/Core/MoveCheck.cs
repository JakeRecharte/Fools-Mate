using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    /// <summary>
    /// Validates a whole plan before MOVE goes ahead. Only reads the board and plan, so it's safe to run any
    /// time, e.g. after every planning click. Friendly collisions, swaps, and attacks are allowed; resolution
    /// handles those.
    /// </summary>
    public static class MoveCheck
    {
        /// <summary>Every problem with the plan. An empty list means MOVE can go ahead.</summary>
        public static List<MoveProblem> Validate(BoardState board, TurnPlan plan)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var problems = new List<MoveProblem>();

            foreach (PlannedMove move in plan.Moves)
            {
                // MoveTargets already knows every movement rule, so reuse it rather than re-checking paths here.
                // This also catches plans gone stale, like a bishop planned through a knight that now stays.
                if (!MoveTargets.GetTargets(board, move.Piece, plan.MovingPieces).Contains(move.To))
                {
                    problems.Add(new MoveProblem(move.Piece, MoveProblemKind.Unreachable,
                        $"{move.Piece} can't reach {move.To} any more."));
                    continue;
                }

                Piece occupant = board.GetPieceAt(move.To);
                bool occupantIsStayingFriend = occupant != null
                    && board.GetColor(occupant) == board.GetColor(move.Piece)
                    && !plan.MovingPieces.Contains(occupant);

                if (occupantIsStayingFriend)
                {
                    problems.Add(new MoveProblem(move.Piece, MoveProblemKind.DestinationOccupied,
                        $"{move.Piece} can't move to {move.To}: {occupant} is staying there."));
                }
            }

            return problems;
        }
    }
}
