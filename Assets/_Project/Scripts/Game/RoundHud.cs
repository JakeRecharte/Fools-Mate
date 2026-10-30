using FoolsMate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Shows the round's progress (round, Tribute, points, turn) and the Round Over panel. Updated by
    /// <see cref="TurnController"/>.
    /// </summary>
    public class RoundHud : MonoBehaviour
    {
        [Tooltip("Optional. Shows which round of the run this is.")]
        [SerializeField] private TMP_Text roundText;

        [SerializeField] private TMP_Text tributeText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text turnText;

        [SerializeField] private GameObject roundOverPanel;
        [SerializeField] private TMP_Text outcomeText;

        [Tooltip("Next Round after a won round, Play Again after a loss or a won game.")]
        [FormerlySerializedAs("playAgainButton")]
        [SerializeField] private Button continueButton;

        [Tooltip("Optional. The Continue Button's label, so it can switch between Next Round and Play Again.")]
        [SerializeField] private TMP_Text continueLabel;

        /// <summary>What the Continue Button does, set when the panel opens.</summary>
        private bool advanceOnContinue;

        /// <summary>Shows the current round, kills, points, and the turn being planned.</summary>
        public void Refresh(RoundScore score)
        {
            if (roundText != null && Run.Current != null)
            {
                roundText.text = $"Round {Run.Current.Round}/{RunProgress.RoundCount}";
            }

            if (tributeText != null)
            {
                tributeText.text = $"Tribute {score.Kills}/{score.Tribute}";
            }

            if (pointsText != null)
            {
                pointsText.text = $"Points {score.Points}";
            }

            if (turnText != null)
            {
                // Capped so the last turn doesn't read 6/5 once the round is over.
                int turn = Mathf.Min(score.TurnsTaken + 1, score.TurnLimit);
                turnText.text = $"Turn {turn}/{score.TurnLimit}";
            }
        }

        public void ShowOutcome(RoundOutcome outcome, RunProgress run)
        {
            bool won = TurnController.IsWin(outcome);
            advanceOnContinue = won && !run.IsFinalRound;

            string result;
            if (!won)
            {
                result = $"You made it to round {run.Round}.";
            }
            else if (run.IsFinalRound)
            {
                result = "You won the game!";
            }
            else
            {
                result = $"You won round {run.Round}!";
            }

            if (outcomeText != null)
            {
                outcomeText.text = $"{TurnController.DescribeOutcome(outcome)}\n{result}";
            }

            if (continueLabel != null)
            {
                continueLabel.text = advanceOnContinue ? "Next Round" : "Play Again";
            }

            if (roundOverPanel != null)
            {
                roundOverPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (roundOverPanel != null)
            {
                roundOverPanel.SetActive(false);
            }
        }

        private void Awake()
        {
            // Hidden here too, so the panel can't cover the board if it was left ticked in the scene.
            Hide();

            if (continueButton == null)
            {
                Debug.LogError("RoundHud: Continue Button isn't assigned.", this);
                return;
            }

            continueButton.onClick.AddListener(Continue);
        }

        private void Continue()
        {
            SfxPlayer.Play(Sfx.Button);

            if (advanceOnContinue)
            {
                Run.Current.Advance();
            }
            else
            {
                // A loss or a won game ends the run, so Play Again starts over from round 1.
                Run.StartNew();
            }

            // A fresh load reruns RoundSetup, which reads the round's turn limit from the run.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
