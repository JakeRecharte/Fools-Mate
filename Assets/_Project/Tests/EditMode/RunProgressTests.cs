using System;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class RunProgressTests
    {
        [Test]
        public void NewRun_StartsAtRoundOneWithTenTurns()
        {
            var run = new RunProgress();

            Assert.AreEqual(1, run.Round);
            Assert.AreEqual(10, run.TurnLimit);
            Assert.IsFalse(run.IsFinalRound);
        }

        [Test]
        public void Advance_DropsTheTurnLimitByTwo()
        {
            var run = new RunProgress();

            run.Advance();
            Assert.AreEqual(2, run.Round);
            Assert.AreEqual(8, run.TurnLimit);

            run.Advance();
            Assert.AreEqual(3, run.Round);
            Assert.AreEqual(6, run.TurnLimit);
        }

        [Test]
        public void RoundFour_IsTheFinalRoundWithFourTurns()
        {
            var run = new RunProgress();
            run.Advance();
            run.Advance();
            run.Advance();

            Assert.AreEqual(4, run.Round);
            Assert.AreEqual(4, run.TurnLimit);
            Assert.IsTrue(run.IsFinalRound);
            Assert.AreEqual(4, RunProgress.RoundCount);
        }

        [Test]
        public void Advance_PastTheFinalRound_Throws()
        {
            var run = new RunProgress();
            run.Advance();
            run.Advance();
            run.Advance();

            Assert.Throws<InvalidOperationException>(() => run.Advance());
        }
    }
}
