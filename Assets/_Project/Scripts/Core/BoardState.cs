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
