using System.Collections.Generic;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class MoveTargetsTests
    {
        private BoardState board;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            nextId = 1;
        }

        private Piece Place(PieceType type, PieceColor color, int file, int rank)
        {
            var piece = new Piece(nextId++, "test", type, new PieceStats(1, 1, 1, 1));
            board.Place(piece, color, new Cell(file, rank));
            return piece;
        }

        private List<Cell> Targets(Piece piece, params Piece[] moving)
        {
            return MoveTargets.GetTargets(board, piece, moving);
        }

        private static Cell C(int file, int rank)
        {
            return new Cell(file, rank);
        }

        // Knight and King

        [Test]
        public void Knight_InCentre_HasEightTargets()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 3, 3);

            Assert.AreEqual(8, Targets(knight).Count);
        }

        [Test]
        public void Knight_InCorner_HasTwoTargets()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 0, 0);

            CollectionAssert.AreEquivalent(new[] { C(1, 2), C(2, 1) }, Targets(knight));
        }

        [Test]
        public void Knight_JumpsOverPieces()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 0, 0);
            Place(PieceType.Pawn, PieceColor.White, 0, 1);
            Place(PieceType.Pawn, PieceColor.White, 1, 1);
            Place(PieceType.Pawn, PieceColor.White, 1, 0);

            Assert.AreEqual(2, Targets(knight).Count);
        }

        [Test]
        public void King_InCentre_HasEightTargets()
        {
            Piece king = Place(PieceType.King, PieceColor.White, 4, 4);

            Assert.AreEqual(8, Targets(king).Count);
        }

        [Test]
        public void Jumps_IncludeOwnAndEnemyCells()
        {
            Piece king = Place(PieceType.King, PieceColor.White, 4, 4);
            Place(PieceType.Pawn, PieceColor.White, 4, 5);
            Place(PieceType.Pawn, PieceColor.Black, 5, 5);

            List<Cell> targets = Targets(king);

            CollectionAssert.Contains(targets, C(4, 5));
            CollectionAssert.Contains(targets, C(5, 5));
        }

        // Sliding pieces

        [Test]
        public void Rook_OnEmptyBoard_HasFourteenTargets()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);

            Assert.AreEqual(14, Targets(rook).Count);
        }

        [Test]
        public void Queen_InCentre_HasTwentySevenTargets()
        {
            Piece queen = Place(PieceType.Queen, PieceColor.White, 3, 3);

            Assert.AreEqual(27, Targets(queen).Count);
        }

        [Test]
        public void Bishop_StopsAtEnemy_AndIncludesItsCell()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 0, 0);
            Place(PieceType.Pawn, PieceColor.Black, 2, 2);

            CollectionAssert.AreEquivalent(new[] { C(1, 1), C(2, 2) }, Targets(bishop));
        }

        [Test]
        public void Bishop_StopsAtOwnStationaryPiece_AndIncludesItsCell()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 0, 0);
            Place(PieceType.Knight, PieceColor.White, 2, 2);

            CollectionAssert.AreEquivalent(new[] { C(1, 1), C(2, 2) }, Targets(bishop));
        }

        [Test]
        public void Bishop_PassesThroughOwnMovingPiece()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 0, 0);
            Piece knight = Place(PieceType.Knight, PieceColor.White, 2, 2);

            List<Cell> targets = Targets(bishop, knight);

            Assert.AreEqual(7, targets.Count);
            CollectionAssert.Contains(targets, C(7, 7));
        }

        [Test]
        public void Bishop_DoesNotPassThroughEnemy_EvenIfListedAsMoving()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 0, 0);
            Piece enemy = Place(PieceType.Pawn, PieceColor.Black, 2, 2);

            CollectionAssert.DoesNotContain(Targets(bishop, enemy), C(3, 3));
        }

        // Pawns

        [Test]
        public void WhitePawn_OnStartRank_MovesOneOrTwoUp()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);

            CollectionAssert.AreEquivalent(new[] { C(4, 2), C(4, 3) }, Targets(pawn));
        }

        [Test]
        public void BlackPawn_OnStartRank_MovesOneOrTwoDown()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.Black, 4, 6);

            CollectionAssert.AreEquivalent(new[] { C(4, 5), C(4, 4) }, Targets(pawn));
        }

        [Test]
        public void Pawn_OffStartRank_MovesOneOnly()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 2);

            CollectionAssert.AreEquivalent(new[] { C(4, 3) }, Targets(pawn));
        }

        [Test]
        public void Pawn_CannotMoveForwardOntoEnemy()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            Place(PieceType.Pawn, PieceColor.Black, 4, 2);

            Assert.IsEmpty(Targets(pawn));
        }

        [Test]
        public void Pawn_CannotDoubleStepOntoEnemy()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            Place(PieceType.Pawn, PieceColor.Black, 4, 3);

            CollectionAssert.AreEquivalent(new[] { C(4, 2) }, Targets(pawn));
        }

        [Test]
        public void Pawn_CapturesDiagonallyOnlyWhenEnemyIsThere()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 2);
            Place(PieceType.Pawn, PieceColor.Black, 5, 3);
            Place(PieceType.Pawn, PieceColor.White, 3, 3);

            List<Cell> targets = Targets(pawn);

            CollectionAssert.Contains(targets, C(5, 3));
            CollectionAssert.DoesNotContain(targets, C(3, 3));
        }

        [Test]
        public void Pawn_ForwardOntoOwnPiece_IsATarget()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 2);
            Place(PieceType.Knight, PieceColor.White, 4, 3);

            CollectionAssert.AreEquivalent(new[] { C(4, 3) }, Targets(pawn));
        }

        [Test]
        public void Pawn_DoubleStep_BlockedByOwnStationaryPiece()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            Place(PieceType.Knight, PieceColor.White, 4, 2);

            CollectionAssert.AreEquivalent(new[] { C(4, 2) }, Targets(pawn));
        }

        [Test]
        public void Pawn_DoubleStep_PassesOwnMovingPiece()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            Piece knight = Place(PieceType.Knight, PieceColor.White, 4, 2);

            CollectionAssert.AreEquivalent(new[] { C(4, 2), C(4, 3) }, Targets(pawn, knight));
        }

        [Test]
        public void Pawn_OnLastRank_HasNoTargets()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Assert.IsEmpty(Targets(pawn));
        }
    }
}
