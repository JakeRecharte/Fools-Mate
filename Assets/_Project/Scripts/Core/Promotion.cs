using System;
using System.Collections.Generic;
using System.Linq;

namespace FoolsMate.Core
{
    /// <summary>
    /// Pawn promotion. A pawn still on its last rank after resolution waits to be promoted. The player swaps in a
    /// piece from the Box during the next planning phase, or the pawn dies when MOVE is pressed. The enemy
    /// promotes right away with a new piece of a random type.
    /// </summary>
    public static class Promotion
    {
        /// <summary>The types the enemy picks from. Later this becomes a catalog for each round.</summary>
        public static readonly IReadOnlyList<PieceType> EnemyChoices =
            new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        /// <summary>The rank a side's pawns promote on: the far side's back rank.</summary>
        public static int LastRank(PieceColor color)
        {
            return Board.FromSideView(new Cell(0, Board.Size - 1), color).Rank;
        }

        /// <summary>One side's pawns standing on their last rank, sorted by ID.</summary>
        public static List<Piece> GetWaitingPawns(BoardState board, PieceColor color)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            int lastRank = LastRank(color);
            return board.Pieces
                .Where(piece => piece.Type == PieceType.Pawn && board.GetColor(piece) == color
                    && board.GetCell(piece).Rank == lastRank)
                .OrderBy(piece => piece.Id)
                .ToList();
        }

        /// <summary>True for a piece type a pawn can promote to. Not a pawn, and never a second King.</summary>
        public static bool CanPromoteTo(PieceType type)
        {
            return type != PieceType.Pawn && type != PieceType.King;
        }

        /// <summary>The pieces in one side's Box that a pawn can promote to, in Box order.</summary>
        public static List<Piece> GetChoices(BoardState board, PieceColor color)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            return board.GetRemoved(color).Where(piece => CanPromoteTo(piece.Type)).ToList();
        }

        /// <summary>
        /// Swaps a waiting pawn for <paramref name="replacement"/>. A piece from the Box comes back at full Health
        /// and the pawn takes its slot in the Box.
        /// </summary>
        public static ResolutionEvent Promote(BoardState board, Piece pawn, Piece replacement)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }

            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            PieceColor color = board.GetColor(pawn);
            if (!GetWaitingPawns(board, color).Contains(pawn))
            {
                throw new InvalidOperationException($"{pawn} isn't a pawn on its last rank.");
            }

            if (!CanPromoteTo(replacement.Type))
            {
                throw new ArgumentException($"A pawn can't promote to a {replacement.Type}.", nameof(replacement));
            }

            Cell cell = board.GetCell(pawn);
            board.Replace(pawn, replacement);
            return ResolutionEvent.Promoted(replacement, color, cell, pawn);
        }

        /// <summary>
        /// Promotes every one of the enemy's waiting pawns to a new piece of a random type from
        /// <see cref="EnemyChoices"/>. <paramref name="createPiece"/> makes the piece, since Core can't see the
        /// piece catalog.
        /// </summary>
        public static List<ResolutionEvent> PromoteAll(BoardState board, PieceColor color, Random random,
            Func<PieceType, Piece> createPiece)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (createPiece == null)
            {
                throw new ArgumentNullException(nameof(createPiece));
            }

            var events = new List<ResolutionEvent>();

            foreach (Piece pawn in GetWaitingPawns(board, color))
            {
                PieceType type = EnemyChoices[random.Next(EnemyChoices.Count)];
                events.Add(Promote(board, pawn, createPiece(type)));
            }

            return events;
        }

        /// <summary>A waiting pawn that wasn't promoted dies. It goes in the Box and counts as nobody's kill.</summary>
        public static ResolutionEvent Forfeit(BoardState board, Piece pawn)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }

            PieceColor color = board.GetColor(pawn);
            if (!GetWaitingPawns(board, color).Contains(pawn))
            {
                throw new InvalidOperationException($"{pawn} isn't a pawn on its last rank.");
            }

            Cell cell = board.GetCell(pawn);
            board.Remove(pawn);
            return ResolutionEvent.Died(pawn, color, cell);
        }
    }
}
