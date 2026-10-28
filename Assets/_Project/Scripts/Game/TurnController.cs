using System.Collections.Generic;
using System.Linq;
using FoolsMate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Runs the turn loop: planning, the Move Check when MOVE is pressed, then resolution, scoring, a redraw,
    /// and either the next turn's planning or the end of the round.
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [SerializeField] private PlanningInput planningInput;
        [SerializeField] private Button moveButton;
        [SerializeField] private TMP_Text warningText;

        [Tooltip("Optional. Shows kills, points, the turn, and the Round Over panel.")]
        [SerializeField] private RoundHud hud;

        private BoardState board;
        private BoardView boardView;
        private PieceColor playerColor;
        private int energy;
        private System.Random random;
        private RoundScore score;

        /// <summary>The turn being planned, counting from 1.</summary>
        private int Turn => score.TurnsTaken + 1;

        /// <summary>
        /// Called by <see cref="RoundSetup"/> once the round's board exists. The random source is shared for the
        /// whole round so one seed replays every Bounce.
        /// </summary>
        public void Begin(BoardState board, BoardView boardView, PieceColor playerColor, int energy,
            System.Random random, RoundScore score)
        {
            this.board = board;
            this.boardView = boardView;
            this.playerColor = playerColor;
            this.energy = energy;
            this.random = random;
            this.score = score;

            if (moveButton != null)
            {
                moveButton.interactable = true;
            }

            RefreshHud();
            StartNextTurn();
        }

        private void StartNextTurn()
        {
            planningInput.Begin(board, playerColor, energy);
            ClearWarnings();
            Debug.Log($"Turn {Turn}/{score.TurnLimit}: plan your moves.", this);
        }

        private void EndRound()
        {
            planningInput.End();

            if (moveButton != null)
            {
                moveButton.interactable = false;
            }

            Debug.Log($"Round over: {DescribeOutcome(score.Outcome)} Kills {score.Kills}/{score.Tribute}, " +
                $"Points {score.Points}.", this);

            if (hud != null)
            {
                hud.ShowOutcome(score.Outcome);
            }
        }

        private void RefreshHud()
        {
            if (hud != null)
            {
                hud.Refresh(score);
            }
        }

        public static string DescribeOutcome(RoundOutcome outcome)
        {
            switch (outcome)
            {
                case RoundOutcome.TributePaid:
                    return "Tribute paid!";
                case RoundOutcome.KingCaptured:
                    return "Enemy King captured!";
                case RoundOutcome.OutOfTurns:
                    return "Out of turns. You lose a life.";
                case RoundOutcome.KingLost:
                    return "Your King fell. You lose a life.";
                default:
                    return outcome.ToString();
            }
        }

        private void Awake()
        {
            // Hooked up in code so a missing reference is a visible error, not a button that does nothing.
            if (moveButton == null)
            {
                Debug.LogError("TurnController: Move Button isn't assigned.", this);
                return;
            }

            moveButton.onClick.AddListener(OnMovePressed);
        }

        private void OnEnable()
        {
            planningInput.PlanChanged += ClearWarnings;
        }

        private void OnDisable()
        {
            planningInput.PlanChanged -= ClearWarnings;
        }

        private void OnMovePressed()
        {
            if (board == null || score.IsOver)
            {
                return;
            }

            TurnPlan plan = planningInput.Plan;
            List<MoveProblem> problems = MoveCheck.Validate(board, plan);

            if (problems.Count > 0)
            {
                ShowWarnings(problems.Select(problem => problem.Message));
                return;
            }

            // Logged before ApplyTurn, which moves the turn counter on.
            List<ResolutionEvent> events = MoveResolver.Resolve(board, plan, random);
            LogEvents(events);

            score.ApplyTurn(events);
            RefreshHud();
            Debug.Log($"Kills {score.Kills}/{score.Tribute} · Points {score.Points} · " +
                $"Turn {score.TurnsTaken}/{score.TurnLimit}", this);

            boardView.Show(board, playerColor);

            if (score.IsOver)
            {
                EndRound();
            }
            else
            {
                StartNextTurn();
            }
        }

        private void LogEvents(List<ResolutionEvent> events)
        {
            if (events.Count == 0)
            {
                Debug.Log($"Turn {Turn}: no moves.", this);
                return;
            }

            Debug.Log($"Turn {Turn}:\n" + string.Join("\n", events), this);
        }

        private void ShowWarnings(IEnumerable<string> messages)
        {
            if (warningText != null)
            {
                warningText.text = string.Join("\n", messages);
            }
        }

        private void ClearWarnings()
        {
            if (warningText != null)
            {
                warningText.text = "";
            }
        }
    }
}
