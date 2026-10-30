using System;

namespace FoolsMate.Core
{
    /// <summary>
    /// Tracks a run across its rounds. For now the only difficulty ramp is the turn limit: it starts at
    /// <see cref="StartingTurnLimit"/> and drops by <see cref="TurnLimitStep"/> each round, and winning the round
    /// at <see cref="FinalTurnLimit"/> wins the game.
    /// </summary>
    public class RunProgress
    {
        public const int StartingTurnLimit = 10;
        public const int TurnLimitStep = 2;
        public const int FinalTurnLimit = 4;

        /// <summary>Rounds in a run: 10, 8, 6, 4 turns.</summary>
        public const int RoundCount = (StartingTurnLimit - FinalTurnLimit) / TurnLimitStep + 1;

        /// <summary>The round being played, counting from 1.</summary>
        public int Round { get; private set; } = 1;

        /// <summary>Worked out from the round, so there's no second counter to keep in sync.</summary>
        public int TurnLimit => StartingTurnLimit - (Round - 1) * TurnLimitStep;

        public bool IsFinalRound => Round >= RoundCount;

        /// <summary>Moves on to the next round after a win.</summary>
        public void Advance()
        {
            if (IsFinalRound)
            {
                throw new InvalidOperationException("The final round has no round after it.");
            }

            Round++;
        }
    }
}
