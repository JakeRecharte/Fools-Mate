using FoolsMate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Shows the round's progress (Tribute, points, turn) and the Round Over panel. Updated by
    /// <see cref="TurnController"/>.
    /// </summary>
    public class RoundHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text tributeText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text turnText;

        [SerializeField] private GameObject roundOverPanel;
        [SerializeField] private TMP_Text outcomeText;
        [SerializeField] private Button playAgainButton;

        /// <summary>Shows the current kills, points, and the turn being planned.</summary>
        public void Refresh(RoundScore score)
        {
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

        public void ShowOutcome(RoundOutcome outcome)
        {
            if (outcomeText != null)
            {
                outcomeText.text = TurnController.DescribeOutcome(outcome);
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

            if (playAgainButton == null)
            {
                Debug.LogError("RoundHud: Play Again Button isn't assigned.", this);
                return;
            }

            playAgainButton.onClick.AddListener(PlayAgain);
        }

        private static void PlayAgain()
        {
            // A fresh load reruns RoundSetup, so the new round gets a new color and seed with nothing to reset.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
