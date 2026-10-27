using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class BoardTests
    {
        [TestCase(0, 0)]
        [TestCase(7, 7)]
        [TestCase(3, 4)]
        public void IsOnBoard_InsideSquares_ReturnsTrue(int file, int rank)
        {
            Assert.IsTrue(Board.IsOnBoard(file, rank));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(8, 0)]
        [TestCase(0, 8)]
        public void IsOnBoard_OutsideSquares_ReturnsFalse(int file, int rank)
        {
            Assert.IsFalse(Board.IsOnBoard(file, rank));
        }

        [TestCase(3, 0)]
        [TestCase(0, 1)]
        [TestCase(7, 7)]
        public void FromSideView_White_ReturnsSameCell(int file, int rank)
        {
            var sideCell = new Cell(file, rank);

            Assert.AreEqual(sideCell, Board.FromSideView(sideCell, PieceColor.White));
        }

        [TestCase(3, 0, 3, 7)] // queen: d1 -> d8
        [TestCase(0, 1, 0, 6)] // a-pawn: a2 -> a7
        [TestCase(7, 7, 7, 0)]
        public void FromSideView_Black_FlipsRankAndKeepsFile(int file, int rank, int expectedFile, int expectedRank)
        {
            Cell real = Board.FromSideView(new Cell(file, rank), PieceColor.Black);

            Assert.AreEqual(new Cell(expectedFile, expectedRank), real);
        }
    }
}
