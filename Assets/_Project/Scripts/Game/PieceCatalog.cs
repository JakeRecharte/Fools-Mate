using System.Collections.Generic;
using FoolsMate.Core;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>Every piece definition in the game, looked up by key or by type.</summary>
    [CreateAssetMenu(fileName = "PieceCatalog", menuName = "Fool's Mate/Piece Catalog")]
    public class PieceCatalog : ScriptableObject
    {
        [SerializeField] private PieceDefinition[] definitions;

        public PieceDefinition GetByKey(string key)
        {
            foreach (PieceDefinition definition in definitions)
            {
                if (definition != null && definition.Key == key)
                {
                    return definition;
                }
            }

            throw new KeyNotFoundException($"No piece definition with key '{key}' in {name}.");
        }

        /// <summary>
        /// Returns the base definition for a piece type. Once variants exist there will be several
        /// definitions per type, and this returns the first one in the list.
        /// </summary>
        public PieceDefinition GetByType(PieceType type)
        {
            foreach (PieceDefinition definition in definitions)
            {
                if (definition != null && definition.Type == type)
                {
                    return definition;
                }
            }

            throw new KeyNotFoundException($"No piece definition of type {type} in {name}.");
        }

        // Unity calls this whenever the asset is edited in the Inspector.
        private void OnValidate()
        {
            if (definitions == null)
            {
                return;
            }

            var seenKeys = new HashSet<string>();

            for (int i = 0; i < definitions.Length; i++)
            {
                PieceDefinition definition = definitions[i];

                if (definition == null)
                {
                    Debug.LogWarning($"{name}: slot {i} is empty.", this);
                    continue;
                }

                if (definition.WhiteSprite == null)
                {
                    Debug.LogWarning($"{name}: {definition.name} has no white sprite.", this);
                }

                if (string.IsNullOrEmpty(definition.Key))
                {
                    Debug.LogWarning($"{name}: {definition.name} has no key.", this);
                }
                else if (!seenKeys.Add(definition.Key))
                {
                    Debug.LogWarning($"{name}: key '{definition.Key}' is used more than once ({definition.name}).", this);
                }
            }
        }
    }
}
