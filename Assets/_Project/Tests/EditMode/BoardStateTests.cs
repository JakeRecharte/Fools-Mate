using System;
using System.Linq;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class BoardStateTests
    {
        private const int MaxHealth = 3;

        private static Piece MakePiece(int id, PieceType type = PieceType.Pawn)
        {
            return new Piece(id, "test", type, new PieceStats(1, 1, 1, MaxHealth));
        }

        /// <summary>An army with just a queen and king on their standard side cells, d1 and e1.</summary>
        private static Army MakeQueenAndKingArmy(out Piece queen, out Piece king)
        {
            queen = MakePiece(1, PieceType.Queen);
            king = MakePiece(2, PieceType.King);

            var army = new Army();
            army.Add(queen, new Cell(3, 0));
            army.Add(king, new Cell(4, 0));
            return army;
        }

        [Test]
        public void PlaceArmy_White_PutsQueenOnD1()
        {
            Army army = MakeQueenAndKingArmy(out Piece queen, out Piece king);
            var board = new BoardState();

            board.PlaceArmy(army, PieceColor.White);

            Assert.AreEqual(new Cell(3, 0), board.GetCell(queen));
            Assert.AreEqual(new Cell(4, 0), board.GetCell(king));
        }

        [Test]
        public void PlaceArmy_Black_PutsQueenOnD8()
        {
            Army army = MakeQueenAndKingArmy(out Piece queen, out Piece king);
            var board = new BoardState();

            board.PlaceArmy(army, PieceColor.Black);

            Assert.AreEqual(new Cell(3, 7), board.GetCell(queen));
            Assert.AreEqual(new Cell(4, 7), board.GetCell(king));
        }

        [Test]
        public void PlaceArmy_SetsColorForEveryPiece()
        {
            Army army = MakeQueenAndKingArmy(out Piece queen, out Piece king);
            var board = new BoardState();

            board.PlaceArmy(army, PieceColor.Black);

            Assert.AreEqual(PieceColor.Black, board.GetColor(queen));
            Assert.AreEqual(PieceColor.Black, board.GetColor(king));
        }

        [Test]
        public void PlaceArmy_BothSides_DoNotCollide()
        {
            Army white = MakeQueenAndKingArmy(out _, out _);
            var black = new Army();
            black.Add(MakePiece(3, PieceType.Queen), new Cell(3, 0));
            black.Add(MakePiece(4, PieceType.King), new Cell(4, 0));
            var board = new BoardState();

            board.PlaceArmy(white, PieceColor.White);
            board.PlaceArmy(black, PieceColor.Black);

            Assert.AreEqual(4, board.Pieces.Count());
        }

        [Test]
        public void Place_PieceCanBeFoundAtItsCell()
        {
            Piece pawn = MakePiece(1);
            var board = new BoardState();

            board.Place(pawn, PieceColor.White, new Cell(4, 1));

            Assert.AreEqual(pawn, board.GetPieceAt(new Cell(4, 1)));
        }

        [Test]
        public void Place_OccupiedCell_Throws()
        {
            var board = new BoardState();
            board.Place(MakePiece(1), PieceColor.White, new Cell(4, 1));

            Assert.Throws<InvalidOperationException>(
                () => board.Place(MakePiece(2), PieceColor.Black, new Cell(4, 1)));
        }

        [Test]
        public void Place_SamePieceTwice_Throws()
        {
            Piece pawn = MakePiece(1);
            var board = new BoardState();
            board.Place(pawn, PieceColor.White, new Cell(4, 1));

            Assert.Throws<InvalidOperationException>(() => board.Place(pawn, PieceColor.White, new Cell(5, 1)));
        }

        [Test]
        public void Place_OffBoardCell_Throws()
        {
            var board = new BoardState();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => board.Place(MakePiece(1), PieceColor.White, new Cell(-1, 0)));
        }

        [Test]
        public void GetPieceAt_EmptyCell_ReturnsNull()
        {
            var board = new BoardState();

            Assert.IsNull(board.GetPieceAt(new Cell(4, 4)));
        }

        [Test]
        public void GetPieceAt_OffBoardCell_ReturnsNull()
        {
            var board = new BoardState();

            Assert.IsNull(board.GetPieceAt(new Cell(8, 8)));
        }

        [Test]
        public void GetCurrentHealth_AfterPlacing_EqualsMaxHealth()
        {
            Piece pawn = MakePiece(1);
            var board = new BoardState();

            board.Place(pawn, PieceColor.White, new Cell(0, 1));

            Assert.AreEqual(MaxHealth, board.GetCurrentHealth(pawn));
        }

        [Test]
        public void GetCell_PieceNotOnBoard_Throws()
        {
            var board = new BoardState();

            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => board.GetCell(MakePiece(1)));
        }
    }
}
