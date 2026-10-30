using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Selects this button while the mouse is over it and deselects it when the mouse leaves, so the
    /// selection is the only highlight whether the menu is driven by the mouse or the keyboard. Used with
    /// <see cref="MenuSelection"/>.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Selectable selectable;

        public bool IsHovered { get; private set; }

        /// <summary>True on frames where the mouse moved, which hands the highlight back to the mouse.</summary>
        public static bool MouseMovedThisFrame()
        {
            return Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Selected in Update once the mouse moves, not here, so a cursor already resting on the button
            // when the menu opens doesn't highlight it.
            IsHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject == gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        private void Update()
        {
            // Also takes the selection back when the keyboard moved it off this button while the mouse sat
            // on it.
            if (IsHovered && MouseMovedThisFrame() && EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject != gameObject)
            {
                SelectIfInteractable();
            }
        }

        private void OnDisable()
        {
            // Hidden buttons get no exit event, so don't let them think they're still hovered.
            IsHovered = false;
        }

        private void SelectIfInteractable()
        {
            if (selectable.IsInteractable())
            {
                selectable.Select();
            }
        }
    }
}
