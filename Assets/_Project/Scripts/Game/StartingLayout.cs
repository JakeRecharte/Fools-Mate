using System;
using FoolsMate.Core;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>
    /// An army written as text rows from the owner's side. Row index is the rank (back rank first) and
    /// character index is the file, so the Q in "RNBQKBNR" is on the d-file.
    /// </summary>
    [CreateAssetMenu(fileName = "StartingLayout", menuName = "Fool's Mate/Starting Layout")]
    public class StartingLayout : ScriptableObject
    {
        private const char EmptyCell = '.';

        [Tooltip("One row per rank from the owner's side, back rank first. Letters: P N B R Q K. '.' is an empty cell.")]
        [SerializeField] private string[] ranks = { "RNBQKBNR", "PPPPPPPP" };

        /// <summary>
        /// Creates a new unique piece for every letter. Pass the run's shared ID source so IDs don't
        /// collide with the other army's.
        /// </summary>
        public Army CreateArmy(PieceCatalog catalog, PieceIdSource ids)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            string problem = FindProblem();
            if (problem != null)
            {
                throw new InvalidOperationException(problem);
            }

            var army = new Army();

            for (int rank = 0; rank < ranks.Length; rank++)
            {
                string row = ranks[rank];

                for (int file = 0; file < row.Length; file++)
                {
                    char letter = row[file];
                    if (letter == EmptyCell)
                    {
                        continue;
                    }

                    PieceTypes.TryFromLetter(letter, out PieceType type);
                    PieceDefinition definition = catalog.GetByType(type);

                    var piece = new Piece(ids.Next(), definition.Key, definition.Type, definition.Stats);
                    army.Add(piece, new Cell(file, rank));
                }
            }

            return army;
        }

        /// <summary>Returns a description of the first thing wrong with the rows, or null if they're valid.</summary>
        private string FindProblem()
        {
            if (ranks == null || ranks.Length == 0)
            {
                return $"{name} has no rows.";
            }

            if (ranks.Length > Board.Size)
            {
                return $"{name} has {ranks.Length} rows; the board only has {Board.Size} ranks.";
            }

            for (int rank = 0; rank < ranks.Length; rank++)
            {
                string row = ranks[rank] ?? "";

                if (row.Length != Board.Size)
                {
                    return $"{name}: row {rank} is \"{row}\" ({row.Length} characters); it needs exactly {Board.Size}.";
                }

                for (int file = 0; file < row.Length; file++)
                {
                    char letter = row[file];
                    if (letter != EmptyCell && !PieceTypes.TryFromLetter(letter, out _))
                    {
                        return $"{name}: unknown piece letter '{letter}' at {new Cell(file, rank)}.";
                    }
                }
            }

            return null;
        }

        // Unity calls this whenever the asset is edited in the Inspector.
        private void OnValidate()
        {
            string problem = FindProblem();
            if (problem != null)
            {
                Debug.LogWarning(problem, this);
            }
        }
    }
}
