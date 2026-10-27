using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class CellTests
    {
        [TestCase(0, 0, "a1")]
        [TestCase(4, 3, "e4")]
        [TestCase(7, 7, "h8")]
        public void ToString_OnBoard_ReturnsChessNotation(int file, int rank, string expected)
        {
            Assert.AreEqual(expected, new Cell(file, rank).ToString());
        }

        [Test]
        public void ToString_OffBoard_ReturnsCoordinates()
        {
            Assert.AreEqual("(9, -1)", new Cell(9, -1).ToString());
        }

        [Test]
        public void Equality_SameCoordinates_AreEqual()
        {
            var a = new Cell(2, 5);
            var b = new Cell(2, 5);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentCoordinates_AreNotEqual()
        {
            Assert.IsTrue(new Cell(2, 5) != new Cell(5, 2));
        }
    }
}
