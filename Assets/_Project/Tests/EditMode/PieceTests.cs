using System;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class PieceTests
    {
        private static readonly PieceStats Stats = new PieceStats(3, 1, 1, 3);

        [Test]
        public void Constructor_IdZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Piece(0, "piece_knight", PieceType.Knight, Stats));
        }

        [Test]
        public void Constructor_TypeNone_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Piece(1, "piece_knight", PieceType.None, Stats));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Constructor_MissingDefinitionKey_Throws(string key)
        {
            Assert.Throws<ArgumentException>(() => new Piece(1, key, PieceType.Knight, Stats));
        }

        [Test]
        public void ToString_ReturnsTypeAndId()
        {
            var piece = new Piece(5, "piece_knight", PieceType.Knight, Stats);

            Assert.AreEqual("Knight #5", piece.ToString());
        }
    }
}
