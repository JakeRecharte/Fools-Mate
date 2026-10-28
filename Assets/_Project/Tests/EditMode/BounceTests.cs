using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class BounceTests
    {
        private BoardState board;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            nextId = 1;
        }

        private Piece Place(int file, int rank)
        {
            var piece = new Piece(nextId++, "test", PieceType.Pawn, new PieceStats(1, 1, 1, 1));
            board.Place(piece, PieceColor.White, new Cell(file, rank));
            return piece;
        }

        [Test]
        public void EmptyBoard_MiddleCell_Has24Candidates()
        {
            // The 5x5 square minus the center.
            Assert.AreEqual(24, Bounce.GetCandidates(board, new Cell(3, 3)).Count);
        }

        [Test]
        public void Corner_OnlyCountsCellsOnTheBoard()
        {
            // From a1 only files a–c and ranks 1–3 are on the board, minus a1 itself.
            Assert.AreEqual(8, Bounce.GetCandidates(board, new Cell(0, 0)).Count);
        }

        [Test]
        public void EmptyCenter_IsNotACandidate()
        {
            CollectionAssert.DoesNotContain(Bounce.GetCandidates(board, new Cell(3, 3)), new Cell(3, 3));
        }

        [Test]
        public void OccupiedCells_AreNotCandidates()
        {
            Place(4, 4);
            Place(5, 5);

            List<Cell> candidates = Bounce.GetCandidates(board, new Cell(3, 3));

            Assert.AreEqual(22, candidates.Count);
            CollectionAssert.DoesNotContain(candidates, new Cell(4, 4));
            CollectionAssert.DoesNotContain(candidates, new Cell(5, 5));
        }

        [Test]
        public void CellsBeyondRange_AreNotCandidates()
        {
            List<Cell> candidates = Bounce.GetCandidates(board, new Cell(3, 3));

            CollectionAssert.DoesNotContain(candidates, new Cell(6, 3));
            CollectionAssert.DoesNotContain(candidates, new Cell(3, 0));
        }

        [Test]
        public void BoxedIn_FindsNoCell()
        {
            for (int file = 0; file <= 2; file++)
            {
                for (int rank = 0; rank <= 2; rank++)
                {
                    Place(file, rank);
                }
            }

            bool found = Bounce.TryFindCell(board, new Cell(0, 0), new Random(1), out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void PickedCell_IsAlwaysACandidate()
        {
            Place(4, 4);
            var center = new Cell(4, 4);
            List<Cell> candidates = Bounce.GetCandidates(board, center);
            var random = new Random(123);

            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(Bounce.TryFindCell(board, center, random, out Cell cell));
                CollectionAssert.Contains(candidates, cell);
            }
        }

        [Test]
        public void SameSeed_PicksSameCell()
        {
            var center = new Cell(3, 3);

            Bounce.TryFindCell(board, center, new Random(42), out Cell first);
            Bounce.TryFindCell(board, center, new Random(42), out Cell second);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => Bounce.TryFindCell(null, new Cell(0, 0), new Random(1), out _));
            Assert.Throws<ArgumentNullException>(() => Bounce.TryFindCell(board, new Cell(0, 0), null, out _));
        }
    }
}
