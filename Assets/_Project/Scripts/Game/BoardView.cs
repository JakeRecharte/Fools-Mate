using System.Collections.Generic;
using System.Linq;
using FoolsMate.Core;
using UnityEngine;
using UnityEngine.Serialization;
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

        [Tooltip("Optional. Drawn on an enemy piece one of your planned pieces moves onto, tinted with the Problem " +
            "Color if the Move Check rejects the move.")]
        [FormerlySerializedAs("placementMarker")]
        [SerializeField] private Sprite playerPlacementMarker;

        [Tooltip("Optional. Drawn on one of your pieces a telegraphed enemy piece moves onto.")]
        [SerializeField] private Sprite enemyPlacementMarker;

        [Tooltip("Optional. Drawn on a cell that you and the enemy both plan to move into, occupied or not.")]
        [SerializeField] private Sprite clashMarker;

        [Tooltip("Opacity of the outline drawn where a piece is planned to move.")]
        [Range(0f, 1f)]
        [SerializeField] private float ghostAlpha = 0.5f;

        [Tooltip("Ghost color for planned moves the Move Check rejects. A warning, not a team color.")]
        [SerializeField] private Color problemColor = new Color(1f, 0.3f, 0.3f, 0.6f);

        [Tooltip("Ghost color for the enemy's telegraphed moves.")]
        [SerializeField] private Color enemyGhostColor = new Color(1f, 0.6f, 0.2f, 0.6f);

        [Tooltip("Optional. Drawn from each planned piece, yours and the enemy's, to its ghost, in the ghost's color.")]
        [SerializeField] private MoveArrow moveArrowPrefab;

        [Header("Captured pieces")]
        [Tooltip("Optional. The slot grid inside the Box, where your captured pieces go. Cell (0, 0) is the top-left slot.")]
        [SerializeField] private Grid boxGrid;

        [Tooltip("Slots per row in the Box. Pieces fill left to right, then the next row down.")]
        [Min(1)]
        [SerializeField] private int boxColumns = 8;

        [Tooltip("Optional. The row above the board where captured enemy pieces line up, left to right.")]
        [SerializeField] private Grid enemyRow;

        [Tooltip("Size of captured pieces, in the Box and the enemy row. 0.5 fits a 32 px piece in a 16 px slot.")]
        [SerializeField] private float capturedScale = 0.5f;

        private readonly List<GameObject> pieceObjects = new List<GameObject>();
        private readonly List<GameObject> markerObjects = new List<GameObject>();
        private readonly List<GameObject> planObjects = new List<GameObject>();
        private readonly List<GameObject> enemyPlanObjects = new List<GameObject>();
        private readonly List<GameObject> enemyMarkerObjects = new List<GameObject>();
        private readonly List<GameObject> capturedObjects = new List<GameObject>();
        private PieceColor playerColor;

        // Kept so the enemy's markers can be redrawn as the player's plan changes, for clashes.
        private BoardState enemyPlanBoard;
        private TurnPlan enemyPlan;
        private readonly HashSet<Cell> playerDestinations = new HashSet<Cell>();

        public void Show(BoardState board, PieceColor playerColor)
        {
            this.playerColor = playerColor;
            Clear();

            foreach (Piece piece in board.Pieces)
            {
                PieceColor color = board.GetColor(piece);
                GameObject pieceObject = CreatePieceObject($"{color} {piece}", piece, color,
                    CellToWorld(board.GetCell(piece)), PieceLayer, 0, Color.white);
                pieceObjects.Add(pieceObject);
            }

            ShowCaptured(board);
        }

        /// <summary>
        /// Draws an arrow from each planned piece to its destination, in the problem color for pieces whose move
        /// the Move Check rejects. A destination holding an enemy piece gets the player marker, and one the enemy
        /// is also moving into gets the clash marker.
        /// </summary>
        public void ShowPlan(BoardState board, TurnPlan plan, ICollection<Piece> problemPieces)
        {
            DestroyAll(planObjects);
            playerDestinations.Clear();

            var ghostColor = new Color(1f, 1f, 1f, ghostAlpha);
            foreach (PlannedMove move in plan.Moves)
            {
                bool hasProblem = problemPieces != null && problemPieces.Contains(move.Piece);
                Color color = hasProblem ? problemColor : ghostColor;
                Sprite marker = null;
                if (IsEnemyDestination(move.To))
                {
                    marker = clashMarker;
                }
                else if (IsOccupiedBy(board, move.To, playerColor.Opposite()))
                {
                    marker = playerPlacementMarker;
                }

                if (marker != null)
                {
                    planObjects.Add(CreateMoveMarker($"Plan {move}", move.To, marker,
                        hasProblem ? problemColor : Color.white));
                }

                playerDestinations.Add(move.To);

                if (moveArrowPrefab != null)
                {
                    planObjects.Add(CreateArrow(move, color));
                }
            }

            ShowEnemyMarkers();
        }

        public void ClearPlan()
        {
            DestroyAll(planObjects);
            playerDestinations.Clear();
            ShowEnemyMarkers();
        }

        /// <summary>
        /// Marks the cell each telegraphed enemy piece is moving to, with an arrow from the piece.
        /// </summary>
        public void ShowEnemyPlan(BoardState board, TurnPlan enemyPlan)
        {
            ClearEnemyPlan();
            enemyPlanBoard = board;
            this.enemyPlan = enemyPlan;

            if (moveArrowPrefab != null)
            {
                foreach (PlannedMove move in enemyPlan.Moves)
                {
                    enemyPlanObjects.Add(CreateArrow(move, enemyGhostColor));
                }
            }

            ShowEnemyMarkers();
        }

        public void ClearEnemyPlan()
        {
            DestroyAll(enemyPlanObjects);
            enemyPlanBoard = null;
            enemyPlan = null;
            ShowEnemyMarkers();
        }

        /// <summary>
        /// Redraws the enemy marker on each of your pieces a telegraphed enemy piece moves onto, leaving out cells
        /// where the player's clash marker stands in.
        /// </summary>
        private void ShowEnemyMarkers()
        {
            DestroyAll(enemyMarkerObjects);

            if (enemyPlan == null || enemyPlacementMarker == null)
            {
                return;
            }

            foreach (PlannedMove move in enemyPlan.Moves)
            {
                if (playerDestinations.Contains(move.To) || !IsOccupiedBy(enemyPlanBoard, move.To, playerColor))
                {
                    continue;
                }

                enemyMarkerObjects.Add(CreateMoveMarker($"Enemy plan {move}", move.To, enemyPlacementMarker,
                    Color.white));
            }
        }

        private bool IsEnemyDestination(Cell cell)
        {
            return enemyPlan != null && enemyPlan.Moves.Any(move => move.To == cell);
        }

        private static bool IsOccupiedBy(BoardState board, Cell cell, PieceColor color)
        {
            Piece piece = board.GetPieceAt(cell);
            return piece != null && board.GetColor(piece) == color;
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
            ClearEnemyPlan();
            DestroyAll(pieceObjects);
            DestroyAll(capturedObjects);
        }

        /// <summary>
        /// Your pieces taken off this round go in the Box, filling its slots row by row; the enemy's line up
        /// above the board. Both are drawn at <see cref="capturedScale"/>, oldest first.
        /// </summary>
        private void ShowCaptured(BoardState board)
        {
            if (boxGrid != null)
            {
                int i = 0;
                foreach (Piece piece in board.GetRemoved(playerColor))
                {
                    // Grid rows count up, so each new row of slots is one step further down.
                    var slot = new Vector3Int(i % boxColumns, -(i / boxColumns), 0);
                    AddCapturedPiece(piece, playerColor, boxGrid.GetCellCenterWorld(slot));
                    i++;
                }
            }

            if (enemyRow != null)
            {
                PieceColor enemyColor = playerColor.Opposite();
                int i = 0;
                foreach (Piece piece in board.GetRemoved(enemyColor))
                {
                    AddCapturedPiece(piece, enemyColor, enemyRow.GetCellCenterWorld(new Vector3Int(i, 0, 0)));
                    i++;
                }
            }
        }

        private void AddCapturedPiece(Piece piece, PieceColor color, Vector3 position)
        {
            GameObject pieceObject = CreatePieceObject($"Captured {color} {piece}", piece, color, position,
                PieceLayer, 0, Color.white);
            pieceObject.transform.localScale = Vector3.one * capturedScale;
            capturedObjects.Add(pieceObject);
        }

        /// <summary>Marks a cell a planned move lands on, with the marker sprite in <paramref name="markerColor"/>.</summary>
        private GameObject CreateMoveMarker(string objectName, Cell cell, Sprite marker, Color markerColor)
        {
            var markerObject = new GameObject(objectName);
            markerObject.transform.SetParent(transform, false);
            markerObject.transform.position = CellToWorld(cell);
            AddSprite(markerObject, marker, PreviewLayer, GhostOrder, markerColor);
            return markerObject;
        }

        /// <summary>
        /// Arrows work in screen cells, so their directions match what's on screen when the board is flipped.
        /// </summary>
        private GameObject CreateArrow(PlannedMove move, Color color)
        {
            MoveArrow arrow = Instantiate(moveArrowPrefab, transform);
            arrow.name = $"Arrow {move}";

            Cell from = ToScreenCell(move.From);
            Cell to = ToScreenCell(move.To);
            arrow.Show(new Vector2Int(from.File, from.Rank), new Vector2Int(to.File, to.Rank), color,
                screenCell => boardTilemap.GetCellCenterWorld(new Vector3Int(screenCell.x, screenCell.y, 0)));

            return arrow.gameObject;
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
        private GameObject CreatePieceObject(string objectName, Piece piece, PieceColor color, Vector3 position,
            string sortingLayer, int baseOrder, Color spriteColor)
        {
            PieceDefinition definition = catalog.GetByKey(piece.DefinitionKey);

            var pieceObject = new GameObject(objectName);
            pieceObject.transform.SetParent(transform, false);
            pieceObject.transform.position = position;
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
