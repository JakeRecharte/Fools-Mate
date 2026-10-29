using System;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>
    /// Draws a planned move as a track of one sprite per cell: an end piece under the moving piece, middle
    /// pieces on the cells it passes, a bend on a knight's corner, and an end piece on the destination
    /// pointing back the way it came. Lives on a prefab so the look (and later, animation) can change without
    /// touching <see cref="BoardView"/>.
    /// </summary>
    public class MoveArrow : MonoBehaviour
    {
        // Same order as the frames (and tags) in the Markers .aseprite files.
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
            new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
        };

        // Bend frames by the two cell edges they open onto, written as the sum of those edges' directions:
        // bottom + right, top + right, top + left, bottom + left.
        private static readonly Vector2Int[] BendCorners =
        {
            new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        [Tooltip("StartAndEndPoints frames in order: Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft.")]
        [SerializeField] private Sprite[] ends = new Sprite[8];

        [Tooltip("Middle frames, in the same order as Ends.")]
        [SerializeField] private Sprite[] middles = new Sprite[8];

        [Tooltip("Bends frames in order: bottom + right, top + right, top + left, bottom + left.")]
        [SerializeField] private Sprite[] bends = new Sprite[4];

        [Tooltip("Sorting layer for the track. Should be below Pieces so the start piece sits under the mover.")]
        [SerializeField] private string sortingLayer = "Arrows";

        [SerializeField] private int sortingOrder;

        /// <summary>
        /// Builds the track between two cells in screen space (after any board flip), so "up" is up on screen.
        /// </summary>
        public void Show(Vector2Int from, Vector2Int to, Color color, Func<Vector2Int, Vector3> cellToWorld)
        {
            Vector2Int delta = to - from;
            int absFile = Math.Abs(delta.x);
            int absRank = Math.Abs(delta.y);

            if ((absFile == 1 && absRank == 2) || (absFile == 2 && absRank == 1))
            {
                ShowKnight(from, delta, color, cellToWorld);
                return;
            }

            // Straight or diagonal: every step is the same.
            var step = new Vector2Int(Math.Sign(delta.x), Math.Sign(delta.y));
            AddPart(ends, step, from, color, cellToWorld);

            for (Vector2Int cell = from + step; cell != to; cell += step)
            {
                AddPart(middles, step, cell, color, cellToWorld);
            }

            AddPart(ends, -step, to, color, cellToWorld);
        }

        /// <summary>Long leg first, then turn: start, middle, bend, end.</summary>
        private void ShowKnight(Vector2Int from, Vector2Int delta, Color color, Func<Vector2Int, Vector3> cellToWorld)
        {
            bool fileIsLong = Math.Abs(delta.x) == 2;
            Vector2Int longStep = fileIsLong ? new Vector2Int(Math.Sign(delta.x), 0) : new Vector2Int(0, Math.Sign(delta.y));
            Vector2Int shortStep = fileIsLong ? new Vector2Int(0, Math.Sign(delta.y)) : new Vector2Int(Math.Sign(delta.x), 0);
            Vector2Int corner = from + 2 * longStep;

            AddPart(ends, longStep, from, color, cellToWorld);
            AddPart(middles, longStep, from + longStep, color, cellToWorld);

            // The corner opens back toward where the knight came from, and toward where it turns.
            int bend = Array.IndexOf(BendCorners, -longStep + shortStep);
            AddSprite(bends[bend], corner, color, cellToWorld);

            AddPart(ends, -shortStep, corner + shortStep, color, cellToWorld);
        }

        private void AddPart(Sprite[] sprites, Vector2Int direction, Vector2Int cell, Color color,
            Func<Vector2Int, Vector3> cellToWorld)
        {
            AddSprite(sprites[Array.IndexOf(Directions, direction)], cell, color, cellToWorld);
        }

        private void AddSprite(Sprite sprite, Vector2Int cell, Color color, Func<Vector2Int, Vector3> cellToWorld)
        {
            var part = new GameObject(sprite != null ? sprite.name : "Arrow part");
            part.transform.SetParent(transform, false);
            part.transform.position = cellToWorld(cell);

            var spriteRenderer = part.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingLayerName = sortingLayer;
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}
