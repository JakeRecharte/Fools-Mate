using System.Collections.Generic;
using FoolsMate.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FoolsMate.Game
{
    /// <summary>
    /// Draws a <see cref="BoardState"/> on the board Tilemap. The only place that knows about the 180°
    /// flip: the board is rotated so the player's pieces are always at the bottom of the screen.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        private const string SortingLayer = "Pieces";
        private const int ColorOrder = 0;
        private const int ShadeOrder = 1;

        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private PieceCatalog catalog;

        private readonly List<GameObject> pieceObjects = new List<GameObject>();
        private PieceColor playerColor;

        public void Show(BoardState board, PieceColor playerColor)
        {
            this.playerColor = playerColor;
            Clear();

            foreach (Piece piece in board.Pieces)
            {
                PieceColor color = board.GetColor(piece);
                PieceDefinition definition = catalog.GetByKey(piece.DefinitionKey);

                var pieceObject = new GameObject($"{color} {piece}");
                pieceObject.transform.SetParent(transform, false);
                pieceObject.transform.position = CellToWorld(board.GetCell(piece));
                AddSprite(pieceObject, definition.GetSprite(color), ColorOrder);

                // Shade is a child so it moves with the piece, and a higher order so it draws on top.
                if (definition.ShadeSprite != null)
                {
                    var shadeObject = new GameObject("Shade");
                    shadeObject.transform.SetParent(pieceObject.transform, false);
                    AddSprite(shadeObject, definition.ShadeSprite, ShadeOrder);
                }

                pieceObjects.Add(pieceObject);
            }
        }

        private static void AddSprite(GameObject target, Sprite sprite, int sortingOrder)
        {
            var spriteRenderer = target.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingLayerName = SortingLayer;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        /// <summary>World position of a cell's centre, rotated 180° when the player is black.</summary>
        public Vector3 CellToWorld(Cell cell)
        {
            int screenFile = cell.File;
            int screenRank = cell.Rank;

            if (playerColor == PieceColor.Black)
            {
                screenFile = Board.Size - 1 - cell.File;
                screenRank = Board.Size - 1 - cell.Rank;
            }

            return boardTilemap.GetCellCenterWorld(new Vector3Int(screenFile, screenRank, 0));
        }

        public void Clear()
        {
            foreach (GameObject pieceObject in pieceObjects)
            {
                Destroy(pieceObject);
            }

            pieceObjects.Clear();
        }
    }
}
