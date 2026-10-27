using System;

namespace FoolsMate.Core
{

    /// <summary>A board cell. File 0 is the a-file and rank 0 is the first rank, so (0, 0) is a1.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public int File { get; }
        public int Rank { get; }

        public Cell(int file, int rank)

        {
            File = file;
            Rank = rank;
        }


        public bool IsOnBoard => Board.IsOnBoard(File, Rank);

        public bool Equals(Cell other)

        {
            return File == other.File && Rank == other.Rank;
        }


        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(File, Rank);
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return IsOnBoard ? $"{(char)('a' + File)}{Rank + 1}" : $"({File}, {Rank})";
        }
    }
}
