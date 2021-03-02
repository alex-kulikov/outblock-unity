using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Denver.Metrics
{
    public class LevelMetricsInput : MonoBehaviour
    {

        [SerializeField]
        private LevelMetrics levelMetrics = null;
        [SerializeField, Space]
        private KeyCode pauseKey = KeyCode.None;
        [SerializeField]
        private string pauseButton = "";
        [SerializeField, Space]
        private KeyCode continueKey = KeyCode.None;
        [SerializeField]
        private string continueButton = "";
        [SerializeField, Space]
        private KeyCode recordKey = KeyCode.None;
        [SerializeField]
        private string recordButton = "";
        [SerializeField, Space]
        private KeyCode stopKey = KeyCode.None;
        [SerializeField]
        private string stopButton = "";

        private void Update()
        {
            if (levelMetrics == null)
                return;

            if (Input.GetKeyDown(pauseKey))
                levelMetrics.Pause();
            if (!string.IsNullOrEmpty(pauseButton) && Input.GetButtonDown(pauseButton))
                levelMetrics.Pause();

            if (Input.GetKeyDown(continueKey))
                levelMetrics.Continue();
            if (!string.IsNullOrEmpty(continueButton) && Input.GetButtonDown(continueButton))
                levelMetrics.Continue();

            if (Input.GetKeyDown(recordKey))
                levelMetrics.Record();
            if (!string.IsNullOrEmpty(recordButton) && Input.GetButtonDown(recordButton))
                levelMetrics.Record();

            if (Input.GetKeyDown(stopKey))
                levelMetrics.Stop();
            if (!string.IsNullOrEmpty(stopButton) && Input.GetButtonDown(stopButton))
                levelMetrics.Stop();
        }

    }
}