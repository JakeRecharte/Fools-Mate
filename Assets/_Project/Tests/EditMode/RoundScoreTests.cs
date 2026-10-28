using System;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class RoundScoreTests
    {
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            nextId = 1;
        }

        private Piece MakePiece(PieceType type, int points)
        {
            return new Piece(nextId++, "test", type, new PieceStats(points, 1, 1, 1));
        }

        /// <summary>A capture made by <paramref name="byColor"/> of a piece belonging to the other side.</summary>
        private ResolutionEvent Capture(PieceType type, int points, PieceColor byColor)
        {
            Piece victim = MakePiece(type, points);
            Piece attacker = MakePiece(PieceType.Rook, 5);
            return ResolutionEvent.Captured(victim, byColor.Opposite(), new Cell(0, 0), 1, attacker, byColor);
        }

        private ResolutionEvent PawnCapturedByWhite()
        {
            return Capture(PieceType.Pawn, 1, PieceColor.White);
        }

        private static RoundScore NewScore(int tribute = 4, int turnLimit = 5)
        {
            return new RoundScore(PieceColor.White, tribute, turnLimit);
        }

        private static readonly ResolutionEvent[] EmptyTurn = new ResolutionEvent[0];

        [Test]
        public void NewScore_StartsEmptyAndInProgress()
        {
            RoundScore score = NewScore();

            Assert.AreEqual(0, score.Kills);
            Assert.AreEqual(0, score.Points);
            Assert.AreEqual(0, score.TurnsTaken);
            Assert.AreEqual(RoundOutcome.InProgress, score.Outcome);
            Assert.IsFalse(score.IsOver);
        }

        [Test]
        public void PlayerCapture_AddsKillAndPoints()
        {
            RoundScore score = NewScore();

            score.ApplyTurn(new[] { Capture(PieceType.Rook, 5, PieceColor.White) });

            Assert.AreEqual(1, score.Kills);
            Assert.AreEqual(5, score.Points);
            Assert.AreEqual(1, score.TurnsTaken);
        }

        [Test]
        public void EveryPiece_CountsAsOneKill_ButPointsDiffer()
        {
            RoundScore score = NewScore();

            score.ApplyTurn(new[]
            {
                Capture(PieceType.Pawn, 1, PieceColor.White),
                Capture(PieceType.Queen, 9, PieceColor.White),
            });

            Assert.AreEqual(2, score.Kills);
            Assert.AreEqual(10, score.Points);
        }

        [Test]
        public void EnemyCapture_DoesNotCount()
        {
            RoundScore score = NewScore();

            score.ApplyTurn(new[] { Capture(PieceType.Rook, 5, PieceColor.Black) });

            Assert.AreEqual(0, score.Kills);
            Assert.AreEqual(0, score.Points);
        }

        [Test]
        public void DiedEvent_DoesNotCount()
        {
            RoundScore score = NewScore();
            Piece enemy = MakePiece(PieceType.Knight, 3);

            score.ApplyTurn(new[] { ResolutionEvent.Died(enemy, PieceColor.Black, new Cell(2, 5)) });

            Assert.AreEqual(0, score.Kills);
            Assert.AreEqual(0, score.Points);
        }

        [Test]
        public void ReachingTribute_PaysIt()
        {
            RoundScore score = NewScore(tribute: 4);

            score.ApplyTurn(new[] { PawnCapturedByWhite(), PawnCapturedByWhite() });
            Assert.AreEqual(RoundOutcome.InProgress, score.Outcome);

            score.ApplyTurn(new[] { PawnCapturedByWhite(), PawnCapturedByWhite() });
            Assert.AreEqual(RoundOutcome.TributePaid, score.Outcome);
            Assert.IsTrue(score.IsOver);
        }

        [Test]
        public void CapturingEnemyKing_WinsImmediately()
        {
            RoundScore score = NewScore(tribute: 4);

            score.ApplyTurn(new[] { Capture(PieceType.King, 0, PieceColor.White) });

            Assert.AreEqual(RoundOutcome.KingCaptured, score.Outcome);
            Assert.AreEqual(1, score.Kills);
        }

        [Test]
        public void LosingKingToBounce_LosesRound()
        {
            RoundScore score = NewScore();
            Piece king = MakePiece(PieceType.King, 0);

            score.ApplyTurn(new[] { ResolutionEvent.Died(king, PieceColor.White, new Cell(4, 0)) });

            Assert.AreEqual(RoundOutcome.KingLost, score.Outcome);
        }

        [Test]
        public void LosingKingToCapture_LosesRound()
        {
            RoundScore score = NewScore();

            score.ApplyTurn(new[] { Capture(PieceType.King, 0, PieceColor.Black) });

            Assert.AreEqual(RoundOutcome.KingLost, score.Outcome);
        }

        [Test]
        public void KingLost_OutranksTributePaid()
        {
            RoundScore score = NewScore(tribute: 4);
            Piece king = MakePiece(PieceType.King, 0);

            score.ApplyTurn(new[]
            {
                PawnCapturedByWhite(),
                PawnCapturedByWhite(),
                PawnCapturedByWhite(),
                PawnCapturedByWhite(),
                ResolutionEvent.Died(king, PieceColor.White, new Cell(4, 0)),
            });

            Assert.AreEqual(RoundOutcome.KingLost, score.Outcome);
            Assert.AreEqual(4, score.Kills, "Kills and points from the turn still count.");
        }

        [Test]
        public void TributePaidOnLastTurn_BeatsOutOfTurns()
        {
            RoundScore score = NewScore(tribute: 1, turnLimit: 1);

            score.ApplyTurn(new[] { PawnCapturedByWhite() });

            Assert.AreEqual(RoundOutcome.TributePaid, score.Outcome);
        }

        [Test]
        public void LastTurnBelowTribute_IsOutOfTurns()
        {
            RoundScore score = NewScore(tribute: 4, turnLimit: 2);

            score.ApplyTurn(EmptyTurn);
            Assert.AreEqual(RoundOutcome.InProgress, score.Outcome);

            score.ApplyTurn(EmptyTurn);
            Assert.AreEqual(RoundOutcome.OutOfTurns, score.Outcome);
            Assert.AreEqual(2, score.TurnsTaken);
        }

        [Test]
        public void ApplyTurn_AfterRoundEnds_Throws()
        {
            RoundScore score = NewScore(turnLimit: 1);
            score.ApplyTurn(EmptyTurn);

            Assert.Throws<InvalidOperationException>(() => score.ApplyTurn(EmptyTurn));
        }

        [Test]
        public void ApplyTurn_NullEvents_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => NewScore().ApplyTurn(null));
        }

        [Test]
        public void InvalidSettings_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NewScore(tribute: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => NewScore(turnLimit: 0));
        }
    }
}
