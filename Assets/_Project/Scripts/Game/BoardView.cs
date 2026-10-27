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

        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private PieceCatalog catalog;

        [Header("Piece Tints")]
        [SerializeField] private Color whiteColor = new Color(0.95f, 0.90f, 0.78f);
        [SerializeField] private Color blackColor = new Color(0.45f, 0.28f, 0.62f);

        private readonly List<GameObject> pieceObjects = new List<GameObject>();
        private PieceColor playerColor;

        public void Show(BoardState board, PieceColor playerColor)
        {
            this.playerColor = playerColor;
            Clear();

            foreach (Piece piece in board.Pieces)
            {
                PieceColor color = board.GetColor(piece);

                var pieceObject = new GameObject($"{color} {piece}");
                pieceObject.transform.SetParent(transform, false);
                pieceObject.transform.position = CellToWorld(board.GetCell(piece));

                var spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = catalog.GetByKey(piece.DefinitionKey).Sprite;
                spriteRenderer.color = color == PieceColor.White ? whiteColor : blackColor;
                spriteRenderer.sortingLayerName = SortingLayer;

                pieceObjects.Add(pieceObject);
            }
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
