using System;
using System.Linq;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class TurnPlanTests
    {
        private static Piece MakePiece(int id)
        {
            return new Piece(id, "test", PieceType.Pawn, new PieceStats(1, 1, 1, 1));
        }

        private static PlannedMove Move(Piece piece, int toRank)
        {
            return new PlannedMove(piece, new Cell(piece.Id, 1), new Cell(piece.Id, toRank));
        }

        [Test]
        public void NewPlan_HasFullEnergyAndNoMoves()
        {
            var plan = new TurnPlan(2);

            Assert.AreEqual(2, plan.EnergyLeft);
            Assert.AreEqual(0, plan.EnergyUsed);
            Assert.IsEmpty(plan.Moves);
        }

        [Test]
        public void TryPlan_WithinEnergy_AddsMoves()
        {
            var plan = new TurnPlan(2);

            Assert.IsTrue(plan.TryPlan(Move(MakePiece(1), 2)));
            Assert.IsTrue(plan.TryPlan(Move(MakePiece(2), 2)));

            Assert.AreEqual(0, plan.EnergyLeft);
            Assert.AreEqual(2, plan.Moves.Count());
        }

        [Test]
        public void TryPlan_NoEnergyLeft_ReturnsFalseAndLeavesPlanUnchanged()
        {
            var plan = new TurnPlan(1);
            plan.TryPlan(Move(MakePiece(1), 2));
            Piece extra = MakePiece(2);

            Assert.IsFalse(plan.TryPlan(Move(extra, 2)));
            Assert.AreEqual(1, plan.Moves.Count());
            CollectionAssert.DoesNotContain(plan.MovingPieces, extra);
        }

        [Test]
        public void TryPlan_SamePieceAgain_ReplacesForFree()
        {
            var plan = new TurnPlan(1);
            Piece pawn = MakePiece(1);
            plan.TryPlan(Move(pawn, 2));

            Assert.IsTrue(plan.TryPlan(Move(pawn, 3)));

            Assert.AreEqual(1, plan.EnergyUsed);
            Assert.IsTrue(plan.TryGetMove(pawn, out PlannedMove move));
            Assert.AreEqual(new Cell(1, 3), move.To);
        }

        [Test]
        public void Cancel_RefundsEnergyAndStopsMoving()
        {
            var plan = new TurnPlan(2);
            Piece pawn = MakePiece(1);
            plan.TryPlan(Move(pawn, 2));

            Assert.IsTrue(plan.Cancel(pawn));

            Assert.AreEqual(2, plan.EnergyLeft);
            CollectionAssert.DoesNotContain(plan.MovingPieces, pawn);
        }

        [Test]
        public void Cancel_PieceWithNoMove_ReturnsFalse()
        {
            var plan = new TurnPlan(2);

            Assert.IsFalse(plan.Cancel(MakePiece(1)));
        }

        [Test]
        public void MovingPieces_ContainsExactlyPlannedPieces()
        {
            var plan = new TurnPlan(2);
            Piece a = MakePiece(1);
            Piece b = MakePiece(2);
            plan.TryPlan(Move(a, 2));
            plan.TryPlan(Move(b, 2));

            CollectionAssert.AreEquivalent(new[] { a, b }, plan.MovingPieces);
        }

        [Test]
        public void TryGetMove_PieceWithNoMove_ReturnsFalse()
        {
            var plan = new TurnPlan(2);

            Assert.IsFalse(plan.TryGetMove(MakePiece(1), out _));
        }

        [Test]
        public void Clear_RemovesAllMoves()
        {
            var plan = new TurnPlan(2);
            plan.TryPlan(Move(MakePiece(1), 2));

            plan.Clear();

            Assert.IsEmpty(plan.Moves);
            Assert.AreEqual(2, plan.EnergyLeft);
        }

        [Test]
        public void ZeroEnergy_IsAllowed_ButNothingCanBePlanned()
        {
            var plan = new TurnPlan(0);

            Assert.IsFalse(plan.TryPlan(Move(MakePiece(1), 2)));
        }

        [Test]
        public void Constructor_NegativeEnergy_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TurnPlan(-1));
        }
    }
}
