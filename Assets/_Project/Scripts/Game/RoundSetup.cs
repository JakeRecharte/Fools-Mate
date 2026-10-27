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

        [Tooltip("Force a color for testing. Random is the real game rule.")]
        [SerializeField] private ColorChoice playerColor = ColorChoice.Random;

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

            boardView.Show(board, rolledColor);
            Debug.Log($"Round start: player is {rolledColor}.", this);
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

            return ok;
        }
    }
}
