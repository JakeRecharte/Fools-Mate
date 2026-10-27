using System;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class ArmyTests
    {
        private static Piece MakePiece(int id, PieceType type = PieceType.Pawn)
        {
            return new Piece(id, "test", type, new PieceStats(1, 1, 1, 3));
        }

        [Test]
        public void Add_ValidPiece_AddsSlot()
        {
            var army = new Army();
            Piece pawn = MakePiece(1);

            army.Add(pawn, new Cell(0, 1));

            Assert.AreEqual(1, army.Slots.Count);
            Assert.AreEqual(pawn, army.Slots[0].Piece);
            Assert.AreEqual(new Cell(0, 1), army.Slots[0].SideCell);
        }

        [Test]
        public void Add_OccupiedSideCell_Throws()
        {
            var army = new Army();
            army.Add(MakePiece(1), new Cell(0, 1));

            Assert.Throws<ArgumentException>(() => army.Add(MakePiece(2), new Cell(0, 1)));
        }

        [Test]
        public void Add_SamePieceTwice_Throws()
        {
            var army = new Army();
            Piece pawn = MakePiece(1);
            army.Add(pawn, new Cell(0, 1));

            Assert.Throws<ArgumentException>(() => army.Add(pawn, new Cell(1, 1)));
        }

        [Test]
        public void Add_OffBoardCell_Throws()
        {
            var army = new Army();

            Assert.Throws<ArgumentOutOfRangeException>(() => army.Add(MakePiece(1), new Cell(8, 0)));
        }

        [Test]
        public void Add_NullPiece_Throws()
        {
            var army = new Army();

            Assert.Throws<ArgumentNullException>(() => army.Add(null, new Cell(0, 1)));
        }
    }
}
