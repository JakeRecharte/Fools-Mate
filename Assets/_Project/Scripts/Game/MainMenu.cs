using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FoolsMate.Game
{
    /// <summary>
    /// The title screen's buttons: Start Run loads the Game scene and Quit closes the game.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        // Loaded by name, since build indices shift whenever scenes are reordered in Build Profiles.
        private const string GameSceneName = "Game";

        [SerializeField] private Button startRunButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (startRunButton == null)
            {
                Debug.LogError("MainMenu: Start Run Button isn't assigned.", this);
            }
            else
            {
                startRunButton.onClick.AddListener(StartRun);
            }

            if (quitButton == null)
            {
                Debug.LogError("MainMenu: Quit Button isn't assigned.", this);
            }
            else
            {
                quitButton.onClick.AddListener(Quit);
            }
        }

        private static void StartRun()
        {
            SfxPlayer.Play(Sfx.Button);
            SceneManager.LoadScene(GameSceneName);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            // Application.Quit does nothing in the Editor, so stop Play Mode instead.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
