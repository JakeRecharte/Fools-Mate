using System.Collections;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>The game's sound effects. Each one has its own clip slot on <see cref="SfxPlayer"/>.</summary>
    public enum Sfx
    {
        Select,
        Plan,
        Cancel,
        Error,
        Move,
        Button,
        Lose
    }

    /// <summary>
    /// Plays the game's sound effects. Put one in every scene; the first one loaded is kept across scene loads
    /// (so a button sound isn't cut off when its scene reloads) and any later copies remove themselves.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        [Tooltip("Clicking one of your pieces.")]
        [SerializeField] private AudioClip select;

        [Tooltip("Planning a move on a marked cell.")]
        [SerializeField] private AudioClip plan;

        [Tooltip("Cancelling a planned move.")]
        [SerializeField] private AudioClip cancel;

        [Tooltip("Planning with no Energy left, or pressing MOVE with a plan that has problems.")]
        [SerializeField] private AudioClip error;

        [Tooltip("Pressing MOVE with a valid plan.")]
        [SerializeField] private AudioClip move;

        [Tooltip("Menu buttons, like Play Again and Start Run.")]
        [SerializeField] private AudioClip button;

        [Tooltip("Losing the round. Played Lose Repeats times in a row.")]
        [SerializeField] private AudioClip lose;

        [SerializeField, Min(1)] private int loseRepeats = 3;

        [Tooltip("Seconds between each Lose repeat.")]
        [SerializeField, Min(0f)] private float loseGap = 0.25f;

        private static SfxPlayer instance;

        private AudioSource source;

        /// <summary>Plays <paramref name="sound"/>. Does nothing if there's no SfxPlayer or its clip is empty.</summary>
        public static void Play(Sfx sound)
        {
            if (instance == null)
            {
                return;
            }

            AudioClip clip = instance.GetClip(sound);
            if (clip == null)
            {
                return;
            }

            if (sound == Sfx.Lose)
            {
                instance.StartCoroutine(instance.PlayRepeated(clip, instance.loseRepeats, instance.loseGap));
            }
            else
            {
                instance.source.PlayOneShot(clip);
            }
        }

        private IEnumerator PlayRepeated(AudioClip clip, int times, float gap)
        {
            for (int i = 0; i < times; i++)
            {
                source.PlayOneShot(clip);
                yield return new WaitForSeconds(gap);
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            source = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }

        private AudioClip GetClip(Sfx sound)
        {
            switch (sound)
            {
                case Sfx.Select:
                    return select;
                case Sfx.Plan:
                    return plan;
                case Sfx.Cancel:
                    return cancel;
                case Sfx.Error:
                    return error;
                case Sfx.Move:
                    return move;
                case Sfx.Button:
                    return button;
                case Sfx.Lose:
                    return lose;
                default:
                    return null;
            }
        }
    }
}
