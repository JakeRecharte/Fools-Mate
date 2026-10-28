using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    /// <summary>
    /// Works out which cells a piece can plan to move to, from the board at the start of the turn plus the
    /// current plan. Your own pieces' cells count as targets because they may move away; the Move Check
    /// rejects them later if they don't. Sliding pieces pass through your own pieces that are planned to move.
    /// </summary>
    public static class MoveTargets
    {
        private static readonly (int File, int Rank)[] KnightJumps =
        {
            (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1), (-2, 1), (-1, 2),
        };

        private static readonly (int File, int Rank)[] Straight = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        private static readonly (int File, int Rank)[] Diagonal = { (1, 1), (1, -1), (-1, 1), (-1, -1) };

        private static readonly (int File, int Rank)[] AllDirections =
        {
            (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1),
        };

        /// <param name="movingPieces">Pieces with a planned move this turn. Null means none.</param>
        public static List<Cell> GetTargets(BoardState board, Piece piece, ICollection<Piece> movingPieces)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            var targets = new List<Cell>();
            Cell from = board.GetCell(piece);
            PieceColor color = board.GetColor(piece);
            ICollection<Piece> moving = movingPieces ?? Array.Empty<Piece>();

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    AddPawnTargets(board, from, color, moving, targets);
                    break;
                case PieceType.Knight:
                    AddJumps(from, KnightJumps, targets);
                    break;
                case PieceType.Bishop:
                    AddSlides(board, from, color, moving, Diagonal, targets);
                    break;
                case PieceType.Rook:
                    AddSlides(board, from, color, moving, Straight, targets);
                    break;
                case PieceType.Queen:
                    AddSlides(board, from, color, moving, AllDirections, targets);
                    break;
                case PieceType.King:
                    AddJumps(from, AllDirections, targets);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(piece), $"{piece} has no movement rules for type {piece.Type}.");
            }

            return targets;
        }

        /// <summary>Knight and King: every on-board cell at a fixed offset, whatever is there.</summary>
        private static void AddJumps(Cell from, (int File, int Rank)[] offsets, List<Cell> targets)
        {
            foreach ((int file, int rank) in offsets)
            {
                var cell = new Cell(from.File + file, from.Rank + rank);
                if (cell.IsOnBoard)
                {
                    targets.Add(cell);
                }
            }
        }

        /// <summary>
        /// Bishop, Rook, Queen: slide until the edge or a piece. A piece's cell is always a target; the slide
        /// only continues past your own pieces that are planned to move.
        /// </summary>
        private static void AddSlides(BoardState board, Cell from, PieceColor color, ICollection<Piece> moving,
            (int File, int Rank)[] directions, List<Cell> targets)
        {
            foreach ((int file, int rank) in directions)
            {
                var cell = new Cell(from.File + file, from.Rank + rank);

                while (cell.IsOnBoard)
                {
                    targets.Add(cell);

                    if (!CanPassThrough(board, cell, color, moving))
                    {
                        break;
                    }

                    cell = new Cell(cell.File + file, cell.Rank + rank);
                }
            }
        }

        /// <summary>
        /// Standard chess pawn: forward one (or two from the starting rank) onto an empty cell or your own
        /// piece, never an enemy; diagonally forward only to capture an enemy.
        /// </summary>
        private static void AddPawnTargets(BoardState board, Cell from, PieceColor color, ICollection<Piece> moving,
            List<Cell> targets)
        {
            int forward = color == PieceColor.White ? 1 : -1;
            int startRank = color == PieceColor.White ? 1 : Board.Size - 2;

            var one = new Cell(from.File, from.Rank + forward);
            if (one.IsOnBoard && !IsEnemy(board, one, color))
            {
                targets.Add(one);

                var two = new Cell(from.File, from.Rank + 2 * forward);
                if (from.Rank == startRank && CanPassThrough(board, one, color, moving) && !IsEnemy(board, two, color))
                {
                    targets.Add(two);
                }
            }

            foreach (int side in new[] { -1, 1 })
            {
                var diagonal = new Cell(from.File + side, from.Rank + forward);
                if (IsEnemy(board, diagonal, color))
                {
                    targets.Add(diagonal);
                }
            }
        }

        /// <summary>Empty, or your own piece that is planned to move away this turn.</summary>
        private static bool CanPassThrough(BoardState board, Cell cell, PieceColor color, ICollection<Piece> moving)
        {
            Piece occupant = board.GetPieceAt(cell);
            return occupant == null || (board.GetColor(occupant) == color && moving.Contains(occupant));
        }

        private static bool IsEnemy(BoardState board, Cell cell, PieceColor color)
        {
            Piece occupant = board.GetPieceAt(cell);
            return occupant != null && board.GetColor(occupant) != color;
        }
    }
}
