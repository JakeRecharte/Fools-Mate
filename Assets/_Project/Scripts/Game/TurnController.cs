using System.Collections.Generic;
using System.Linq;
using FoolsMate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Runs the turn loop: planning, the Move Check when MOVE is pressed, then resolution, a redraw, and the
    /// next turn's planning.
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [SerializeField] private PlanningInput planningInput;
        [SerializeField] private Button moveButton;
        [SerializeField] private TMP_Text warningText;

        private BoardState board;
        private BoardView boardView;
        private PieceColor playerColor;
        private int energy;
        private System.Random random;
        private int turn;

        /// <summary>
        /// Called by <see cref="RoundSetup"/> once the round's board exists. The random source is shared for the
        /// whole round so one seed replays every Bounce.
        /// </summary>
        public void Begin(BoardState board, BoardView boardView, PieceColor playerColor, int energy,
            System.Random random)
        {
            this.board = board;
            this.boardView = boardView;
            this.playerColor = playerColor;
            this.energy = energy;
            this.random = random;
            turn = 0;
            StartNextTurn();
        }

        private void StartNextTurn()
        {
            turn++;
            planningInput.Begin(board, playerColor, energy);
            ClearWarnings();
            Debug.Log($"Turn {turn}: plan your moves.", this);
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
            if (board == null)
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

            List<ResolutionEvent> events = MoveResolver.Resolve(board, plan, random);
            LogEvents(events);

            boardView.Show(board, playerColor);
            StartNextTurn();
        }

        private void LogEvents(List<ResolutionEvent> events)
        {
            if (events.Count == 0)
            {
                Debug.Log($"Turn {turn}: no moves.", this);
                return;
            }

            Debug.Log($"Turn {turn}:\n" + string.Join("\n", events), this);
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
