using System;

namespace FoolsMate.Core
{
    /// <summary>
    /// A unique piece that lasts the whole run. Its color and square are set each round on the board,
    /// not stored here.
    /// </summary>
    public class Piece
    {
        public int Id { get; }
        public string DefinitionKey { get; }
        public PieceType Type { get; }
        public PieceStats Stats { get; private set; }

        public Piece(int id, string definitionKey, PieceType type, PieceStats stats)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Piece IDs start at 1; 0 means an empty square.");
            }

            if (string.IsNullOrEmpty(definitionKey))
            {
                throw new ArgumentException("A piece needs a definition key, e.g. \"piece_knight\".", nameof(definitionKey));
            }

            if (type == PieceType.None)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "A piece can't have type None.");
            }

            Id = id;
            DefinitionKey = definitionKey;
            Type = type;
            Stats = stats;
        }

        public override string ToString()
        {
            return $"{Type} #{Id}";
        }
    }
}
