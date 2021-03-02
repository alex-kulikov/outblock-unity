using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OutBlock
{

    /// <summary>
    /// Loads the victory scene.
    /// </summary>
    public class VictoryTrigger : SceneTrigger
    {

        private string playAgainScene;

        /// <summary>
        /// Last scene that can be played again.
        /// </summary>
        public static string PlayAgainScene { get; private set; }

        private void Start()
        {
#if UNITY_EDITOR
            PlayAgainScene = SceneManager.GetActiveScene().path;
#else
        PlayAgainScene = SceneManager.GetActiveScene().name;
#endif
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
        }

    }
}