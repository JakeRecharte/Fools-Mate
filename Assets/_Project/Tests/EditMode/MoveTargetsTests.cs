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

            List<Cell> targets = Targets(bishop, enemy);

            CollectionAssert.Contains(targets, C(2, 2), "The enemy's cell is still a target.");
            CollectionAssert.DoesNotContain(targets, C(3, 3));
        }

        [Test]
        public void Queen_StopsWhereOwnPawnLands()
        {
            // The pawn steps from d2 to d3: the queen passes d2 but stops at d3.
            Piece queen = Place(PieceType.Queen, PieceColor.White, 3, 0);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 3, 1);
            var plan = new TurnPlan(2);
            plan.TryPlan(new PlannedMove(pawn, C(3, 1), C(3, 2)));

            List<Cell> targets = MoveTargets.GetTargets(board, queen, plan);

            CollectionAssert.Contains(targets, C(3, 1));
            CollectionAssert.Contains(targets, C(3, 2), "Moving onto the landing cell is a friendly collision.");
            CollectionAssert.DoesNotContain(targets, C(3, 3));
        }

        [Test]
        public void PlanOverload_IgnoresThePiecesOwnMove()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            var plan = new TurnPlan(2);
            plan.TryPlan(new PlannedMove(rook, C(0, 0), C(0, 3)));

            CollectionAssert.Contains(MoveTargets.GetTargets(board, rook, plan), C(0, 7));
        }

        // Enemy plan

        [Test]
        public void EnemyMovingAway_ItsCellIsNotATarget()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Piece enemy = Place(PieceType.Rook, PieceColor.Black, 0, 5);
            var enemyPlan = new TurnPlan(1);
            enemyPlan.TryPlan(new PlannedMove(enemy, C(0, 5), C(0, 1)));

            List<Cell> targets = MoveTargets.GetTargets(board, rook, new TurnPlan(1), enemyPlan);

            CollectionAssert.DoesNotContain(targets, C(0, 5), "No trading places with a leaving enemy.");
            CollectionAssert.Contains(targets, C(0, 1), "Its destination is still open.");
        }

        [Test]
        public void EnemyStaying_ItsCellIsStillATarget()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Place(PieceType.Rook, PieceColor.Black, 0, 5);
            Piece other = Place(PieceType.Pawn, PieceColor.Black, 7, 6);
            var enemyPlan = new TurnPlan(1);
            enemyPlan.TryPlan(new PlannedMove(other, C(7, 6), C(7, 5)));

            CollectionAssert.Contains(MoveTargets.GetTargets(board, rook, new TurnPlan(1), enemyPlan), C(0, 5));
        }

        [Test]
        public void EnemyMovingAway_StillBlocksSlides()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Piece enemy = Place(PieceType.Pawn, PieceColor.Black, 0, 3);
            var enemyPlan = new TurnPlan(1);
            enemyPlan.TryPlan(new PlannedMove(enemy, C(0, 3), C(0, 2)));

            List<Cell> targets = MoveTargets.GetTargets(board, rook, new TurnPlan(1), enemyPlan);

            CollectionAssert.DoesNotContain(targets, C(0, 3));
            CollectionAssert.DoesNotContain(targets, C(0, 4));
        }

        [Test]
        public void PawnCapture_OntoEnemyMovingAway_IsNotATarget()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 3, 3);
            Piece enemy = Place(PieceType.Knight, PieceColor.Black, 4, 4);
            var enemyPlan = new TurnPlan(1);
            enemyPlan.TryPlan(new PlannedMove(enemy, C(4, 4), C(3, 3)));

            CollectionAssert.DoesNotContain(MoveTargets.GetTargets(board, pawn, new TurnPlan(1), enemyPlan), C(4, 4));
        }

        [Test]
        public void Pawn_DoubleStep_BlockedByPieceLandingInFront()
        {
            // A knight lands on e3 from the side, so the e-pawn can't double-step over it.
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            Piece knight = Place(PieceType.Knight, PieceColor.White, 6, 0);
            var plan = new TurnPlan(2);
            plan.TryPlan(new PlannedMove(knight, C(6, 0), C(4, 2)));

            List<Cell> targets = MoveTargets.GetTargets(board, pawn, plan);

            CollectionAssert.Contains(targets, C(4, 2));
            CollectionAssert.DoesNotContain(targets, C(4, 3));
        }

        [Test]
        public void Pawn_DoubleStep_NotBlockedByRookFollowingBehind()
        {
            // Black h-pawn h7 to h5 with the rook following h8 to h6: the rook lands on a cell the pawn passes,
            // but it's behind the pawn, so the pawn isn't overtaking it.
            Piece pawn = Place(PieceType.Pawn, PieceColor.Black, 7, 6);
            Piece rook = Place(PieceType.Rook, PieceColor.Black, 7, 7);
            var plan = new TurnPlan(2);
            plan.TryPlan(new PlannedMove(pawn, C(7, 6), C(7, 4)));
            plan.TryPlan(new PlannedMove(rook, C(7, 7), C(7, 5)));

            CollectionAssert.Contains(MoveTargets.GetTargets(board, pawn, plan), C(7, 4));
            CollectionAssert.Contains(MoveTargets.GetTargets(board, rook, plan), C(7, 5));
            CollectionAssert.DoesNotContain(MoveTargets.GetTargets(board, rook, plan), C(7, 3));
        }

        [Test]
        public void Bishop_NotBlockedByQueenFollowingBehind()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 2, 2);
            Piece queen = Place(PieceType.Queen, PieceColor.White, 0, 0);
            var plan = new TurnPlan(2);
            plan.TryPlan(new PlannedMove(queen, C(0, 0), C(4, 4)));

            CollectionAssert.Contains(MoveTargets.GetTargets(board, bishop, plan), C(6, 6));
        }

        [Test]
        public void Bishop_IsBlockedByStayingEnemy()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 0, 0);
            Place(PieceType.Pawn, PieceColor.Black, 2, 2);

            List<Cell> targets = Targets(bishop);

            CollectionAssert.Contains(targets, C(2, 2));
            CollectionAssert.DoesNotContain(targets, C(3, 3));
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
