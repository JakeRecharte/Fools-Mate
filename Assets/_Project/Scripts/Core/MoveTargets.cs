using System;
using System.Collections.Generic;
using System.Linq;

namespace FoolsMate.Core
{
    /// <summary>
    /// Works out which cells a piece can plan to move to, from the board at the start of the turn plus the
    /// current plan. Your own pieces' cells count as targets because they may move away; the Move Check
    /// rejects them later if they don't. Sliding pieces pass through your own pieces that are planned to move,
    /// and can't overtake them: a cell another of your pieces moves into blocks the slide, unless that piece is
    /// following behind on the same line. Enemy pieces always block, and an enemy telegraphed to move away
    /// takes its cell with it: you can't move onto it, so you can't just trade places with it.
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

        /// <summary>
        /// Targets for a piece given the rest of its side's plan: it can slide past the other planned movers,
        /// but can't overtake them. The piece's own planned move is ignored.
        /// </summary>
        /// <param name="enemyPlan">The other side's telegraphed plan. Cells its movers are leaving aren't
        /// targets. Null means no enemy plan.</param>
        public static List<Cell> GetTargets(BoardState board, Piece piece, TurnPlan plan, TurnPlan enemyPlan = null)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            List<PlannedMove> otherMoves = plan.Moves.Where(move => move.Piece != piece).ToList();
            List<Cell> targets = GetTargets(board, piece, plan.MovingPieces, otherMoves);

            if (enemyPlan != null)
            {
                targets.RemoveAll(cell => IsEnemyLeaving(board, cell, enemyPlan));
            }

            return targets;
        }

        /// <summary>True if an enemy is standing on <paramref name="cell"/> and is telegraphed to move away.</summary>
        public static bool IsEnemyLeaving(BoardState board, Cell cell, TurnPlan enemyPlan)
        {
            Piece occupant = board.GetPieceAt(cell);
            return occupant != null && enemyPlan.MovingPieces.Contains(occupant);
        }

        /// <param name="movingPieces">Your pieces with a planned move this turn. Null means none.</param>
        /// <param name="otherMoves">Your other pieces' planned moves, for where they land. Null means none.</param>
        public static List<Cell> GetTargets(BoardState board, Piece piece, ICollection<Piece> movingPieces = null,
            ICollection<PlannedMove> otherMoves = null)
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
            var blocking = new Blocking(board, color, from, movingPieces, otherMoves);

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    AddPawnTargets(board, from, color, blocking, targets);
                    break;
                case PieceType.Knight:
                    AddJumps(from, KnightJumps, targets);
                    break;
                case PieceType.Bishop:
                    AddSlides(from, blocking, Diagonal, targets);
                    break;
                case PieceType.Rook:
                    AddSlides(from, blocking, Straight, targets);
                    break;
                case PieceType.Queen:
                    AddSlides(from, blocking, AllDirections, targets);
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
        /// only continues past your own pieces that are planned to move, and stops at cells your pieces move into.
        /// </summary>
        private static void AddSlides(Cell from, Blocking blocking, (int File, int Rank)[] directions,
            List<Cell> targets)
        {
            foreach ((int file, int rank) in directions)
            {
                var cell = new Cell(from.File + file, from.Rank + rank);

                while (cell.IsOnBoard)
                {
                    targets.Add(cell);

                    if (!blocking.CanPassThrough(cell, file, rank))
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
        private static void AddPawnTargets(BoardState board, Cell from, PieceColor color, Blocking blocking,
            List<Cell> targets)
        {
            int forward = color == PieceColor.White ? 1 : -1;
            int startRank = color == PieceColor.White ? 1 : Board.Size - 2;

            var one = new Cell(from.File, from.Rank + forward);
            if (one.IsOnBoard && !IsEnemy(board, one, color))
            {
                targets.Add(one);

                var two = new Cell(from.File, from.Rank + 2 * forward);
                if (from.Rank == startRank && blocking.CanPassThrough(one, 0, forward) && !IsEnemy(board, two, color))
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

        /// <summary>What a slide (or a pawn's double step) can pass over.</summary>
        private readonly struct Blocking
        {
            private readonly BoardState board;
            private readonly PieceColor color;
            private readonly Cell from;
            private readonly ICollection<Piece> moving;
            private readonly ICollection<PlannedMove> otherMoves;

            public Blocking(BoardState board, PieceColor color, Cell from, ICollection<Piece> moving,
                ICollection<PlannedMove> otherMoves)
            {
                this.board = board;
                this.color = color;
                this.from = from;
                this.moving = moving ?? Array.Empty<Piece>();
                this.otherMoves = otherMoves ?? Array.Empty<PlannedMove>();
            }

            /// <summary>
            /// Empty, or your own piece that is planned to move away, and not a cell where another of your
            /// pieces lands ahead of this one.
            /// </summary>
            public bool CanPassThrough(Cell cell, int file, int rank)
            {
                foreach (PlannedMove move in otherMoves)
                {
                    if (move.To == cell && !IsFollowing(move.From, file, rank))
                    {
                        return false;
                    }
                }

                Piece occupant = board.GetPieceAt(cell);
                return occupant == null || (board.GetColor(occupant) == color && moving.Contains(occupant));
            }

            /// <summary>
            /// True if <paramref name="otherFrom"/> is directly behind this piece on the line it's moving along,
            /// e.g. a rook following its pawn up the file. A follower can't be overtaken, so it doesn't block.
            /// </summary>
            private bool IsFollowing(Cell otherFrom, int file, int rank)
            {
                int fileGap = from.File - otherFrom.File;
                int rankGap = from.Rank - otherFrom.Rank;

                // Directions are -1, 0, or 1, so multiplying by one is the same as dividing by it.
                int steps = file != 0 ? fileGap * file : rankGap * rank;
                return steps >= 1 && fileGap == steps * file && rankGap == steps * rank;
            }
        }

        private static bool IsEnemy(BoardState board, Cell cell, PieceColor color)
        {
            Piece occupant = board.GetPieceAt(cell);
            return occupant != null && board.GetColor(occupant) != color;
        }
    }
}
