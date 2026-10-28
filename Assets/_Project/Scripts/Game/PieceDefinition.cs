using FoolsMate.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace FoolsMate.Game
{
    [CreateAssetMenu(fileName = "PieceDefinition", menuName = "Fool's Mate/Piece Definition")]
    public class PieceDefinition : ScriptableObject
    {
        [Tooltip("Stable ID saved in runs, e.g. piece_knight. Don't rename once used.")]
        [SerializeField] private string key;
        [SerializeField] private PieceType type;

        [Header("Sprites")]
        [FormerlySerializedAs("sprite")]
        [SerializeField] private Sprite whiteSprite;

        [Tooltip("Leave empty to use the white sprite for both sides.")]
        [SerializeField] private Sprite blackSprite;

        [Tooltip("Optional shading drawn on top of either color.")]
        [SerializeField] private Sprite shadeSprite;

        [Header("Stats")]
        [SerializeField] private int points;
        [SerializeField] private int priority;
        [SerializeField] private int damage;
        [SerializeField] private int health;

        public string Key => key;
        public PieceType Type => type;
        public Sprite WhiteSprite => whiteSprite;
        public Sprite ShadeSprite => shadeSprite;
        public PieceStats Stats => new PieceStats(points, priority, damage, health);

        public Sprite GetSprite(PieceColor color)
        {
            // Unity objects need an explicit null check; ?? doesn't see destroyed or missing references.
            if (color == PieceColor.Black && blackSprite != null)
            {
                return blackSprite;
            }

            return whiteSprite;
        }
    }
}
