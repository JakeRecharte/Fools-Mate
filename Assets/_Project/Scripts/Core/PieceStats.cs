namespace FoolsMate.Core
{
    public readonly struct PieceStats
    {
        /// <summary>Points the opponent earns for capturing this piece.</summary>
        public int Points { get; }

        /// <summary>Decides which piece wins the square when pieces clash.</summary>
        public int Priority { get; }

        /// <summary>Damage the piece deals when it attacks or collides.</summary>
        public int Damage { get; }

        /// <summary>Maximum Health. The piece starts each round with this much.</summary>
        public int Health { get; }

        public PieceStats(int points, int priority, int damage, int health)
        {
            Points = points;
            Priority = priority;
            Damage = damage;
            Health = health;
        }
    }
}
