using System;
using System.Collections.Generic;
using System.Linq;

namespace FoolsMate.Core
{
    /// <summary>
    /// One round's board: which piece is on which cell, each piece's color this round, and its current
    /// Health. Created fresh each round, so Health starts back at max without a reset step.
    /// </summary>
    public class BoardState
    {
        private const int Empty = 0;

        /// <summary>Piece IDs indexed [file, rank]. 0 means the cell is empty.</summary>
        private readonly int[,] pieceIds = new int[Board.Size, Board.Size];

        private readonly Dictionary<int, PlacedPiece> placed = new Dictionary<int, PlacedPiece>();

        public IEnumerable<Piece> Pieces => placed.Values.Select(p => p.Piece);

        public void PlaceArmy(Army army, PieceColor color)
        {
            if (army == null)
            {
                throw new ArgumentNullException(nameof(army));
            }

            foreach (ArmySlot slot in army.Slots)
            {
                Place(slot.Piece, color, Board.FromSideView(slot.SideCell, color));
            }
        }

        public void Place(Piece piece, PieceColor color, Cell cell)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (!cell.IsOnBoard)
            {
                throw new ArgumentOutOfRangeException(nameof(cell), $"{cell} is off the board.");
            }

            if (pieceIds[cell.File, cell.Rank] != Empty)
            {
                throw new InvalidOperationException($"{cell} is already taken by {GetPieceAt(cell)}.");
            }

            if (placed.ContainsKey(piece.Id))
            {
                throw new InvalidOperationException($"{piece} is already on the board at {placed[piece.Id].Cell}.");
            }

            pieceIds[cell.File, cell.Rank] = piece.Id;
            placed.Add(piece.Id, new PlacedPiece(piece, color, cell));
        }

        /// <summary>
        /// Takes a piece off the grid for the moment but keeps it on the board, so moves can resolve at the
        /// same time. Its <see cref="GetCell"/> stays at the cell it left until it lands.
        /// </summary>
        public void Lift(Piece piece)
        {
            PlacedPiece entry = Find(piece);
            Cell cell = entry.Cell;

            // Only clear the cell if it's still ours, so lifting twice can't wipe out a piece that landed there.
            if (pieceIds[cell.File, cell.Rank] == piece.Id)
            {
                pieceIds[cell.File, cell.Rank] = Empty;
            }
        }

        /// <summary>Puts a piece already on this board onto a cell, usually after <see cref="Lift"/>.</summary>
        public void Land(Piece piece, Cell cell)
        {
            PlacedPiece entry = Find(piece);

            if (!cell.IsOnBoard)
            {
                throw new ArgumentOutOfRangeException(nameof(cell), $"{cell} is off the board.");
            }

            int occupantId = pieceIds[cell.File, cell.Rank];
            if (occupantId != Empty && occupantId != piece.Id)
            {
                throw new InvalidOperationException($"{cell} is already taken by {GetPieceAt(cell)}.");
            }

            // Clear the old cell in case the piece wasn't lifted first.
            Cell oldCell = entry.Cell;
            if (pieceIds[oldCell.File, oldCell.Rank] == piece.Id)
            {
                pieceIds[oldCell.File, oldCell.Rank] = Empty;
            }

            pieceIds[cell.File, cell.Rank] = piece.Id;
            entry.Cell = cell;
        }

        /// <summary>Lowers a piece's Health, never below 0. Returns true if its Health is now 0.</summary>
        public bool TakeDamage(Piece piece, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage can't be negative.");
            }

            PlacedPiece entry = Find(piece);
            entry.CurrentHealth = Math.Max(0, entry.CurrentHealth - amount);
            return entry.CurrentHealth == 0;
        }

        /// <summary>Takes a piece off the board for good, e.g. when it's captured or dies.</summary>
        public void Remove(Piece piece)
        {
            PlacedPiece entry = Find(piece);
            Cell cell = entry.Cell;

            if (pieceIds[cell.File, cell.Rank] == piece.Id)
            {
                pieceIds[cell.File, cell.Rank] = Empty;
            }

            placed.Remove(piece.Id);
        }

        /// <summary>Returns the piece on the cell, or null if the cell is empty or off the board.</summary>
        public Piece GetPieceAt(Cell cell)
        {
            if (!cell.IsOnBoard)
            {
                return null;
            }

            int id = pieceIds[cell.File, cell.Rank];
            return id == Empty ? null : placed[id].Piece;
        }

        public PieceColor GetColor(Piece piece)
        {
            return Find(piece).Color;
        }

        public Cell GetCell(Piece piece)
        {
            return Find(piece).Cell;
        }

        public int GetCurrentHealth(Piece piece)
        {
            return Find(piece).CurrentHealth;
        }

        private PlacedPiece Find(Piece piece)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (!placed.TryGetValue(piece.Id, out PlacedPiece entry))
            {
                throw new KeyNotFoundException($"{piece} isn't on this board.");
            }

            return entry;
        }

        /// <summary>A piece's state for this round. A class so changes to Cell and Health stick.</summary>
        private class PlacedPiece
        {
            public Piece Piece { get; }
            public PieceColor Color { get; }
            public Cell Cell { get; set; }
            public int CurrentHealth { get; set; }

            public PlacedPiece(Piece piece, PieceColor color, Cell cell)
            {
                Piece = piece;
                Color = color;
                Cell = cell;
                CurrentHealth = piece.Stats.Health;
            }
        }
    }
}
