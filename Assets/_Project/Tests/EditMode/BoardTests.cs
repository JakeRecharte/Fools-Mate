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
    }
}
