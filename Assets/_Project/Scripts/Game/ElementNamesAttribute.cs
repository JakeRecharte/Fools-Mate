using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FoolsMate.Game
{
    /// <summary>
    /// Labels a serialized array's elements in the Inspector ("Up", "UpRight", ...) instead of "Element 0",
    /// "Element 1", ... so it's clear which sprite goes in which slot.
    /// </summary>
    public class ElementNamesAttribute : PropertyAttribute
    {
        public readonly string[] Names;

        public ElementNamesAttribute(params string[] names)
        {
            Names = names;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ElementNamesAttribute))]
    internal class ElementNamesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Unity draws the attribute once per element; the index is at the end of the path: "ends.Array.data[3]".
            string path = property.propertyPath;
            string[] names = ((ElementNamesAttribute)attribute).Names;

            if (path.EndsWith("]"))
            {
                int open = path.LastIndexOf('[');
                if (int.TryParse(path.Substring(open + 1, path.Length - open - 2), out int index) && index < names.Length)
                {
                    label = new GUIContent(names[index], label.tooltip);
                }
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
#endif
}
