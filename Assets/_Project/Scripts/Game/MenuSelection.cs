using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// Switches a menu's highlight between the keyboard and the mouse. Nothing is highlighted until a
    /// navigate key is pressed, which selects the last button used (or the default one). Moving the mouse
    /// clears that highlight unless the mouse is on the selected button. Give each button a
    /// <see cref="SelectOnHover"/>, and leave the EventSystem's First Selected empty.
    /// </summary>
    // Runs after the EventSystem, so the key press that wakes the menu isn't also read as a move away
    // from the button it just selected.
    [DefaultExecutionOrder(100)]
    public class MenuSelection : MonoBehaviour
    {
        [SerializeField] private Selectable defaultSelection;

        private GameObject lastSelected;

        private void OnEnable()
        {
            // Each time the menu opens, the keyboard starts from the default button again.
            lastSelected = null;
        }

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

            GameObject selected = eventSystem.currentSelectedGameObject;

            if (SelectOnHover.MouseMovedThisFrame())
            {
                if (selected != null && !IsHovered(selected))
                {
                    eventSystem.SetSelectedGameObject(null);
                }
            }
            else if (selected == null && NavigatePressed(eventSystem))
            {
                SelectForKeyboard(eventSystem);
            }

            if (eventSystem.currentSelectedGameObject != null)
            {
                lastSelected = eventSystem.currentSelectedGameObject;
            }
        }

        private void SelectForKeyboard(EventSystem eventSystem)
        {
            if (lastSelected != null && lastSelected.activeInHierarchy)
            {
                eventSystem.SetSelectedGameObject(lastSelected);
            }
            else if (defaultSelection != null)
            {
                eventSystem.SetSelectedGameObject(defaultSelection.gameObject);
            }
        }

        private static bool IsHovered(GameObject selected)
        {
            return selected.TryGetComponent(out SelectOnHover hover) && hover.IsHovered;
        }

        private static bool NavigatePressed(EventSystem eventSystem)
        {
            // Reads the UI module's own Move action, so this follows whatever keys and sticks it's bound to.
            return eventSystem.currentInputModule is InputSystemUIInputModule module
                && module.move != null && module.move.action != null
                && module.move.action.WasPressedThisFrame();
        }
    }
}
