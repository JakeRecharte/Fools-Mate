using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    public readonly struct ArmySlot
    {
        public Piece Piece { get; }
        public Cell SideCell { get; }

        public ArmySlot(Piece piece, Cell sideCell)
        {
            Piece = piece;
            SideCell = sideCell;
        }
    }

    /// <summary>
    /// One side's pieces and their starting cells, seen from that side: rank 0 is its back rank,
    /// whatever color it gets. The real cells are worked out when the round's colors are rolled.
    /// </summary>
    public class Army
    {
        private readonly List<ArmySlot> slots = new List<ArmySlot>();

        public IReadOnlyList<ArmySlot> Slots => slots;

        public void Add(Piece piece, Cell sideCell)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (!sideCell.IsOnBoard)
            {
                throw new ArgumentOutOfRangeException(nameof(sideCell), $"{sideCell} is off the board.");
            }

            foreach (ArmySlot slot in slots)
            {
                if (slot.Piece == piece)
                {
                    throw new ArgumentException($"{piece} is already in this army.", nameof(piece));
                }

                if (slot.SideCell == sideCell)
                {
                    throw new ArgumentException($"{sideCell} is already taken by {slot.Piece}.", nameof(sideCell));
                }
            }

            slots.Add(new ArmySlot(piece, sideCell));
        }
    }
}
