using FoolsMate.Core;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>
    /// Sets up a round on Play: builds both armies, rolls the player's color, places the armies on a fresh
    /// board, and shows it.
    /// </summary>
    public class RoundSetup : MonoBehaviour
    {
        private enum ColorChoice
        {
            Random,
            White,
            Black,
        }

        [SerializeField] private PieceCatalog catalog;
        [SerializeField] private StartingLayout playerLayout;
        [SerializeField] private StartingLayout enemyLayout;
        [SerializeField] private BoardView boardView;
        [SerializeField] private TurnController turnController;

        [Tooltip("Force a color for testing. Random is the real game rule.")]
        [SerializeField] private ColorChoice playerColor = ColorChoice.Random;

        [Tooltip("Moves the player can plan each turn. Level 1 is 2.")]
        [Min(0)]
        [SerializeField] private int energyPerTurn = 2;

        [Tooltip("Enemy pieces to capture to win the round. Level 1 is 4.")]
        [Min(1)]
        [SerializeField] private int tribute = 4;

        [Tooltip("Turns to pay the Tribute before losing a life. Level 1 is 5.")]
        [Min(1)]
        [SerializeField] private int turnLimit = 5;

        [Tooltip("0 picks a new seed each round. Set a number to replay the same Bounces while testing.")]
        [SerializeField] private int randomSeed;

        private BoardState board;

        private void Start()
        {
            if (!HasReferences())
            {
                return;
            }

            // One ID source for both armies so no two pieces share an ID.
            var ids = new PieceIdSource();
            Army playerArmy = playerLayout.CreateArmy(catalog, ids);
            Army enemyArmy = enemyLayout.CreateArmy(catalog, ids);

            PieceColor rolledColor = RollPlayerColor();

            board = new BoardState();
            board.PlaceArmy(playerArmy, rolledColor);
            board.PlaceArmy(enemyArmy, rolledColor.Opposite());

            // Core rules use System.Random, since Core can't see Unity. Logged so a strange round can be replayed.
            int seed = randomSeed != 0 ? randomSeed : Random.Range(1, int.MaxValue);
            Debug.Log($"Round start: player is {rolledColor}, seed {seed}.", this);

            var score = new RoundScore(rolledColor, tribute, turnLimit);

            boardView.Show(board, rolledColor);
            turnController.Begin(board, boardView, rolledColor, energyPerTurn, new System.Random(seed), score);
        }

        private PieceColor RollPlayerColor()
        {
            switch (playerColor)
            {
                case ColorChoice.White:
                    return PieceColor.White;
                case ColorChoice.Black:
                    return PieceColor.Black;
                default:
                    return Random.value < 0.5f ? PieceColor.White : PieceColor.Black;
            }
        }

        private bool HasReferences()
        {
            bool ok = true;

            if (catalog == null)
            {
                Debug.LogError("RoundSetup: Catalog isn't assigned.", this);
                ok = false;
            }

            if (playerLayout == null)
            {
                Debug.LogError("RoundSetup: Player Layout isn't assigned.", this);
                ok = false;
            }

            if (enemyLayout == null)
            {
                Debug.LogError("RoundSetup: Enemy Layout isn't assigned.", this);
                ok = false;
            }

            if (boardView == null)
            {
                Debug.LogError("RoundSetup: Board View isn't assigned.", this);
                ok = false;
            }

            if (turnController == null)
            {
                Debug.LogError("RoundSetup: Turn Controller isn't assigned.", this);
                ok = false;
            }

            return ok;
        }
    }
}
