using System;
using System.Collections.Generic;
using System.Linq;

namespace FoolsMate.Core
{
    /// <summary>
    /// Rule-based enemy: picks its moves for the turn from the board at the start of the turn, before the
    /// player plans. Greedy plus safety: every candidate move is scored by what it captures minus what it
    /// risks, and the best ones are taken.
    /// </summary>
    public static class EnemyPlanner
    {
        /// <summary>
        /// What a piece is worth to the planner. Kings score 0 points, so they get a high value here instead,
        /// so the enemy goes for the player's King and keeps its own out of danger.
        /// </summary>
        public const int KingValue = 100;

        /// <summary>Plans up to <paramref name="moves"/> moves for <paramref name="enemyColor"/>.</summary>
        public static TurnPlan Plan(BoardState board, PieceColor enemyColor, int moves, Random random)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var plan = new TurnPlan(moves);
            HashSet<Cell> dangerCells = GetDangerCells(board, enemyColor);
            List<(PlannedMove Move, int Score)> candidates = GetCandidates(board, enemyColor, dangerCells);

            // Shuffled before the (stable) sort so equal scores are picked at random, not always in board order.
            Shuffle(candidates, random);
            var takenCells = new HashSet<Cell>();

            foreach ((PlannedMove move, int _) in candidates.OrderByDescending(candidate => candidate.Score))
            {
                if (plan.EnergyLeft <= 0)
                {
                    break;
                }

                // One move per piece, and never two enemy pieces into one cell, which would Bounce them both.
                if (plan.MovingPieces.Contains(move.Piece) || takenCells.Contains(move.To))
                {
                    continue;
                }

                plan.TryPlan(move);
                takenCells.Add(move.To);
            }

            return plan;
        }

        public static int ValueOf(Piece piece)
        {
            return piece.Type == PieceType.King ? KingValue : piece.Stats.Points;
        }

        /// <summary>
        /// Cells the other side could move into this turn. Moves happen at the same time, so landing on one
        /// risks a collision now or a capture next turn.
        /// </summary>
        private static HashSet<Cell> GetDangerCells(BoardState board, PieceColor enemyColor)
        {
            var cells = new HashSet<Cell>();

            foreach (Piece piece in board.Pieces.Where(piece => board.GetColor(piece) != enemyColor).ToList())
            {
                cells.UnionWith(MoveTargets.GetTargets(board, piece));
            }

            return cells;
        }

        private static List<(PlannedMove Move, int Score)> GetCandidates(BoardState board, PieceColor enemyColor,
            HashSet<Cell> dangerCells)
        {
            var candidates = new List<(PlannedMove, int)>();

            // Sorted by ID so the same board and seed always give the same plan.
            foreach (Piece piece in board.Pieces.Where(piece => board.GetColor(piece) == enemyColor)
                         .OrderBy(piece => piece.Id).ToList())
            {
                Cell from = board.GetCell(piece);

                foreach (Cell to in MoveTargets.GetTargets(board, piece))
                {
                    Piece occupant = board.GetPieceAt(to);

                    // MoveTargets allows your own cells in case that piece moves away. The enemy doesn't plan
                    // around its own movers, so those cells are off limits.
                    if (occupant != null && board.GetColor(occupant) == enemyColor)
                    {
                        continue;
                    }

                    int score = occupant != null ? ValueOf(occupant) : 0;
                    if (dangerCells.Contains(to))
                    {
                        score -= ValueOf(piece);
                    }

                    candidates.Add((new PlannedMove(piece, from, to), score));
                }
            }

            return candidates;
        }

        private static void Shuffle<T>(List<T> items, Random random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
