using FoolsMate.Core;
using UnityEngine;

namespace FoolsMate.Game
{
    /// <summary>
    /// Holds the current run between scene loads. Each round reloads the Game scene, which wipes every
    /// MonoBehaviour, so the run lives in a static instead.
    /// </summary>
    public static class Run
    {
        /// <summary>The run in progress, or null before one starts.</summary>
        public static RunProgress Current { get; private set; }

        public static void StartNew()
        {
            Current = new RunProgress();
        }

        // Domain reload is off in Enter Play Mode Options, so statics survive between Play sessions. Cleared
        // here so pressing Play doesn't pick up last session's round.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Current = null;
        }
    }
}
