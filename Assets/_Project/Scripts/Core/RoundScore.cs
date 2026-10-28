using System;
using System.Collections.Generic;

namespace FoolsMate.Core
{
    public enum RoundOutcome
    {
        /// <summary>The round goes on.</summary>
        InProgress,

        /// <summary>The player reached the Tribute's kill count.</summary>
        TributePaid,

        /// <summary>The player captured the enemy King.</summary>
        KingCaptured,

        /// <summary>The turn limit ran out before the Tribute was paid. Costs a life.</summary>
        OutOfTurns,

        /// <summary>The player's King left the board. Costs a life.</summary>
        KingLost,
    }

    /// <summary>
    /// Tracks one round's progress for the player: kills toward the Tribute, points for the shop, turns taken,
    /// and whether the round is over. Kills win the round; points are only currency.
    /// </summary>
    public class RoundScore
    {
        private readonly PieceColor playerColor;

        public RoundScore(PieceColor playerColor, int tribute, int turnLimit)
        {
            if (tribute < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(tribute), "The Tribute must be at least 1 kill.");
            }

            if (turnLimit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(turnLimit), "The turn limit must be at least 1.");
            }

            this.playerColor = playerColor;
            Tribute = tribute;
            TurnLimit = turnLimit;
            Outcome = RoundOutcome.InProgress;
        }

        /// <summary>Enemy pieces the player captured. Counts toward the Tribute.</summary>
        public int Kills { get; private set; }

        /// <summary>Shop currency earned from captures. Doesn't count toward the Tribute.</summary>
        public int Points { get; private set; }

        public int Tribute { get; }
        public int TurnLimit { get; }
        public int TurnsTaken { get; private set; }
        public RoundOutcome Outcome { get; private set; }

        public bool IsOver => Outcome != RoundOutcome.InProgress;

        /// <summary>Counts one resolved turn's kills and points, then decides whether the round is over.</summary>
        public void ApplyTurn(IEnumerable<ResolutionEvent> events)
        {
            if (IsOver)
            {
                throw new InvalidOperationException($"The round is already over ({Outcome}).");
            }

            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            TurnsTaken++;
            bool kingCaptured = false;
            bool kingLost = false;

            foreach (ResolutionEvent e in events)
            {
                if (e.Kind == ResolutionEventKind.Captured && e.ByColor == playerColor)
                {
                    Kills++;
                    Points += PointsFor(e);

                    if (e.Piece.Type == PieceType.King)
                    {
                        kingCaptured = true;
                    }
                }

                bool removed = e.Kind == ResolutionEventKind.Captured || e.Kind == ResolutionEventKind.Died;
                if (removed && e.Color == playerColor && e.Piece.Type == PieceType.King)
                {
                    kingLost = true;
                }
            }

            // Most important result first: losing your King outranks everything else on the same turn.
            if (kingLost)
            {
                Outcome = RoundOutcome.KingLost;
            }
            else if (kingCaptured)
            {
                Outcome = RoundOutcome.KingCaptured;
            }
            else if (Kills >= Tribute)
            {
                Outcome = RoundOutcome.TributePaid;
            }
            else if (TurnsTaken >= TurnLimit)
            {
                Outcome = RoundOutcome.OutOfTurns;
            }
        }

        /// <summary>Points a capture earns. Upgrades like Double Points will hook in here.</summary>
        private static int PointsFor(ResolutionEvent capture)
        {
            return capture.Piece.Stats.Points;
        }
    }
}
