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
        private const string PieceLayer = "Pieces";
        private const string PreviewLayer = "Previews";

        // Within the Previews layer: planned-move ghosts underneath, target markers on top.
        private const int GhostOrder = 0;
        private const int MarkerOrder = 2;

        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private PieceCatalog catalog;

        [Header("Markers")]
        [SerializeField] private Sprite targetMarker;

        [Tooltip("Optional marker on the selected piece's own cell.")]
        [SerializeField] private Sprite selectedMarker;

        [Tooltip("Opacity of the outline drawn where a piece is planned to move.")]
        [Range(0f, 1f)]
        [SerializeField] private float ghostAlpha = 0.5f;

        [Tooltip("Ghost color for planned moves the Move Check rejects. A warning, not a team color.")]
        [SerializeField] private Color problemColor = new Color(1f, 0.3f, 0.3f, 0.6f);

        private readonly List<GameObject> pieceObjects = new List<GameObject>();
        private readonly List<GameObject> markerObjects = new List<GameObject>();
        private readonly List<GameObject> planObjects = new List<GameObject>();
        private PieceColor playerColor;

        public void Show(BoardState board, PieceColor playerColor)
        {
            this.playerColor = playerColor;
            Clear();

            foreach (Piece piece in board.Pieces)
            {
                PieceColor color = board.GetColor(piece);
                GameObject pieceObject = CreatePieceObject($"{color} {piece}", piece, color, board.GetCell(piece),
                    PieceLayer, 0, Color.white);
                pieceObjects.Add(pieceObject);
            }
        }

        /// <summary>
        /// Draws a see-through copy of each planned piece on its destination cell, in the problem color for
        /// pieces whose move the Move Check rejects.
        /// </summary>
        public void ShowPlan(BoardState board, TurnPlan plan, ICollection<Piece> problemPieces)
        {
            ClearPlan();

            var ghostColor = new Color(1f, 1f, 1f, ghostAlpha);
            foreach (PlannedMove move in plan.Moves)
            {
                bool hasProblem = problemPieces != null && problemPieces.Contains(move.Piece);
                GameObject ghost = CreatePieceObject($"Plan {move}", move.Piece, board.GetColor(move.Piece), move.To,
                    PreviewLayer, GhostOrder, hasProblem ? problemColor : ghostColor);
                planObjects.Add(ghost);
            }
        }

        public void ClearPlan()
        {
            DestroyAll(planObjects);
        }

        /// <summary>Marks the selected piece's cell and every cell it can move to.</summary>
        public void ShowTargets(Cell selected, IEnumerable<Cell> targets)
        {
            ClearTargets();

            if (selectedMarker != null)
            {
                AddMarker("Selected", selected, selectedMarker);
            }

            foreach (Cell target in targets)
            {
                AddMarker($"Target {target}", target, targetMarker);
            }
        }

        public void ClearTargets()
        {
            DestroyAll(markerObjects);
        }

        /// <summary>World position of a cell's centre, rotated 180° when the player is black.</summary>
        public Vector3 CellToWorld(Cell cell)
        {
            Cell screenCell = ToScreenCell(cell);
            return boardTilemap.GetCellCenterWorld(new Vector3Int(screenCell.File, screenCell.Rank, 0));
        }

        /// <summary>The real cell under a world position, or false if it's off the board.</summary>
        public bool TryWorldToCell(Vector3 world, out Cell cell)
        {
            Vector3Int screenCell = boardTilemap.WorldToCell(world);

            // A 180° rotation undoes itself, so the same conversion goes back from screen to real.
            cell = ToScreenCell(new Cell(screenCell.x, screenCell.y));
            return cell.IsOnBoard;
        }

        public void Clear()
        {
            ClearTargets();
            ClearPlan();
            DestroyAll(pieceObjects);
        }

        private Cell ToScreenCell(Cell cell)
        {
            if (playerColor == PieceColor.Black)
            {
                return new Cell(Board.Size - 1 - cell.File, Board.Size - 1 - cell.Rank);
            }

            return cell;
        }

        /// <summary>A piece's color sprite with its shade as a child, so the shade moves with it.</summary>
        private GameObject CreatePieceObject(string objectName, Piece piece, PieceColor color, Cell cell,
            string sortingLayer, int baseOrder, Color spriteColor)
        {
            PieceDefinition definition = catalog.GetByKey(piece.DefinitionKey);

            var pieceObject = new GameObject(objectName);
            pieceObject.transform.SetParent(transform, false);
            pieceObject.transform.position = CellToWorld(cell);
            AddSprite(pieceObject, definition.GetSprite(color), sortingLayer, baseOrder, spriteColor);

            if (definition.ShadeSprite != null)
            {
                var shadeObject = new GameObject("Shade");
                shadeObject.transform.SetParent(pieceObject.transform, false);
                AddSprite(shadeObject, definition.ShadeSprite, sortingLayer, baseOrder + 1, spriteColor);
            }

            return pieceObject;
        }

        private void AddMarker(string markerName, Cell cell, Sprite sprite)
        {
            var markerObject = new GameObject(markerName);
            markerObject.transform.SetParent(transform, false);
            markerObject.transform.position = CellToWorld(cell);
            AddSprite(markerObject, sprite, PreviewLayer, MarkerOrder, Color.white);
            markerObjects.Add(markerObject);
        }

        private static void AddSprite(GameObject target, Sprite sprite, string sortingLayer, int sortingOrder,
            Color spriteColor)
        {
            var spriteRenderer = target.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingLayerName = sortingLayer;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = spriteColor;
        }

        private static void DestroyAll(List<GameObject> objects)
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }

            objects.Clear();
        }
    }
}
