using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    /// <summary>
    /// Finds where a Bounced piece lands: a random empty cell within <see cref="Range"/> tiles of the cell it
    /// tried to enter. Only picks the cell; the caller moves the piece, or kills it if there's no room.
    /// </summary>
    public static class Bounce
    {
        public const int Range = 2;

        /// <summary>Picks a random empty cell near <paramref name="center"/>. False if every cell is taken.</summary>
        public static bool TryFindCell(BoardState board, Cell center, Random random, out Cell cell)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<Cell> candidates = GetCandidates(board, center);
            if (candidates.Count == 0)
            {
                cell = default;
                return false;
            }

            cell = candidates[random.Next(candidates.Count)];
            return true;
        }

        /// <summary>
        /// Every empty on-board cell in the square around <paramref name="center"/>, in a fixed order. The center
        /// itself never counts: a Bounced piece shouldn't land on the cell it just lost.
        /// </summary>
        public static List<Cell> GetCandidates(BoardState board, Cell center)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var cells = new List<Cell>();
            for (int df = -Range; df <= Range; df++)
            {
                for (int dr = -Range; dr <= Range; dr++)
                {
                    var candidate = new Cell(center.File + df, center.Rank + dr);
                    if (candidate == center || !candidate.IsOnBoard || board.GetPieceAt(candidate) != null)
                    {
                        continue;
                    }

                    cells.Add(candidate);
                }
            }

            return cells;
        }
    }
}
