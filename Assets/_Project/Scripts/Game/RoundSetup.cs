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

        [Tooltip("Moves the enemy makes each turn. Level 1 is 2.")]
        [Min(0)]
        [SerializeField] private int enemyMovesPerTurn = 2;

        [Tooltip("Enemy pieces to capture to win the round. Level 1 is 4.")]
        [Min(1)]
        [SerializeField] private int tribute = 4;

        [Tooltip("0 picks a new seed each round. Set a number to replay the same Bounces while testing.")]
        [SerializeField] private int randomSeed;

        private BoardState board;

        private void Start()
        {
            if (!HasReferences())
            {
                return;
            }

            // Pressing Play straight from the Game scene skips the menu, so start the run here.
            if (Run.Current == null)
            {
                Run.StartNew();
            }

            RunProgress run = Run.Current;

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
            Debug.Log($"Round {run.Round}/{RunProgress.RoundCount} start: player is {rolledColor}, " +
                $"{run.TurnLimit} turns, seed {seed}.", this);

            var score = new RoundScore(rolledColor, tribute, run.TurnLimit);

            boardView.Show(board, rolledColor);

            // Enemy pawns promote to new pieces, from the same ID source so they don't collide either.
            turnController.Begin(board, boardView, rolledColor, energyPerTurn, enemyMovesPerTurn,
                new System.Random(seed), score, type => catalog.CreatePiece(type, ids));
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
