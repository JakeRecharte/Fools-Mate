using FoolsMate.Core;
using UnityEngine;

namespace FoolsMate.Game
{
    [CreateAssetMenu(fileName = "PieceDefinition", menuName = "Fool's Mate/Piece Definition")]
    public class PieceDefinition : ScriptableObject
    {
        [Tooltip("Stable ID saved in runs, e.g. piece_knight. Don't rename once used.")]
        [SerializeField] private string key;
        [SerializeField] private PieceType type;
        [SerializeField] private Sprite sprite;

        [Header("Stats")]
        [SerializeField] private int points;
        [SerializeField] private int priority;
        [SerializeField] private int damage;
        [SerializeField] private int health;

        public string Key => key;
        public PieceType Type => type;
        public Sprite Sprite => sprite;
        public PieceStats Stats => new PieceStats(points, priority, damage, health);
    }
}
