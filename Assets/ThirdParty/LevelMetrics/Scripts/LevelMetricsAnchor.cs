using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Denver.Metrics
{
    public class LevelMetricsAnchor : MonoBehaviour
    {

        [SerializeField]
        private Recordable.Config recordable = default;
        [SerializeField, Space]
        private List<Signal.Config> signals = new List<Signal.Config>();

        private LevelMetrics levelMetrics = null;

        private bool initialized;

        private void Start()
        {
            Init();
        }

        private void OnValidate()
        {
            FillProperties();
        }

        private void FillProperties()
        {
            if (recordable != null && recordable.ObjectToRecord == null)
                recordable.ObjectToRecord = transform;

            foreach (Signal.Config signal in signals)
            {
                if (signal.LookUpObject == null)
                    signal.LookUpObject = transform;
            }
        }

        private void Init()
        {
            if (initialized)
                return;

            levelMetrics = LevelMetrics.Instance;

            if (!levelMetrics || !enabled)
                return;

            levelMetrics.AddRecordable(recordable);

            foreach(Signal.Config signal in signals)
            {
                levelMetrics.AddSignal(signal);
            }

            initialized = true;
        }

        /// <summary>
        /// Stop all recordings.
        /// </summary>
        public void StopRecording()
        {
            recordable.Record = false;
            foreach (Signal.Config signal in signals)
                signal.Record = false;
        }

        /// <summary>
        /// Resumes all recording.
        /// </summary>
        public void ResumeRecording()
        {
            recordable.Record = true;
            foreach (Signal.Config signal in signals)
                signal.Record = true;
        }
		
		public void SendSignal(string name)
        {
            Signal.Config signal = signals.FirstOrDefault(x => x.Name == name);
            if (signal != null)
            {
                SendSignal(signals.IndexOf(signal));
            }
        }

        public void SendSignal(int index)
        {
            if (!initialized || !enabled)
                return;

            if (index < 0 || index >= signals.Count)
            {
                Debug.LogError("Index is out of the bounds!", this);
                return;
            }

            levelMetrics.SendSignal(signals[index].Name, signals[index].ObjectName);
        }

    }
}