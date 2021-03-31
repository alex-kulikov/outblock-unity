using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Denver.Metrics
{

    /// <summary>
    /// Main class for the LevelMetrics component.
    /// </summary>
    public partial class LevelMetrics : MonoBehaviour
    {

        [SerializeField, Header("Main")]
        private bool recording = true;
        [SerializeField, Tooltip("Show GUI with controls.")]
        private bool showGUI = true;
        [SerializeField, Tooltip("GUI position")]
        private GUIPositions guiPosition = GUIPositions.LeftTop;
        [SerializeField, Tooltip("Explicit GUI position")]
        private Vector2 explicitPosition = Vector2.zero;
        [SerializeField, Header("Signals")]
        private List<Signal.Config> signals = new List<Signal.Config>();
        [SerializeField, Space, Header("Recordable")]
        private List<Recordable.Config> recordables = new List<Recordable.Config>();
        [SerializeField, Header("IO"), Tooltip("Overwrite or append the file?")]
        private WriteModes writeMode = WriteModes.Overwrite;
        [SerializeField]
        private bool recordOnBuild = false;
        [SerializeField, TextArea, Tooltip("This commentary will be written with the files")]
        private string commentary = "";

        private bool pause;
        private bool DontRecordInBuild => !Application.isEditor && !recordOnBuild;

        private Dictionary<string, List<Signal.Runtime>> runtimeSignals = new Dictionary<string, List<Signal.Runtime>>();
        private Dictionary<string, Recordable.Runtime> runtimePaths = new Dictionary<string, Recordable.Runtime>();

        /// <summary>
        /// Singletone. Only one instance of this class should be present in the scene!
        /// </summary>
        public static LevelMetrics Instance { get; private set; }

        #region MonoBehaviour
        private void Awake()
        {
            //Singletone pattern
            if (!Instance)
            {
                Instance = this;
                Init();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < signals.Count; i++)
            {
                if (string.IsNullOrEmpty(signals[i].Name))
                    signals[i].Name = "Signal";
            }

            for (int i = 0; i < recordables.Count; i++)
            {
                if (string.IsNullOrEmpty(recordables[i].Name))
                    recordables[i].Name = "Recordable";
            }
        }

        //Write all runtime data
        private void OnDisable()
        {
            Write();
        }

        //Signals key activation and path recording
        private void Update()
        {
            if (pause || !recording)
                return;

            if (DontRecordInBuild)
                return;

            foreach (Signal.Config signal in signals)
            {
                if (!signal.ActivateOnKey || !signal.LookUpObject)
                    continue;

                if (!string.IsNullOrEmpty(signal.Button) && Input.GetButtonDown(signal.Button))
                    SendSignal(signals.IndexOf(signal));

                if (Input.GetKeyDown(signal.Key))
                    SendSignal(signals.IndexOf(signal));
            }

            for (int i = 0; i < recordables.Count; i++)
            {
                if (!recordables[i].ObjectToRecord || !recordables[i].Record)
                    continue;

                if (recordables[i].UpdateTimer())
                {
                    Recordable.Runtime runtimePath = runtimePaths[recordables[i].ObjectToRecord.name];
                    if (runtimePath.Data.Count <= 0)
                        runtimePath.Add(new Recordable.Data(Time.time, recordables[i].ObjectToRecord.position, false));
                    else
                    {
                        Recordable.Data lastPoint = runtimePath.Data.Last();
                        if (Vector3.Distance(lastPoint.pos, recordables[i].ObjectToRecord.position) > 0.2f)
                        {
                            runtimePath.Add(new Recordable.Data(Time.time, recordables[i].ObjectToRecord.position, false));
                        }
                    }
                }
            }
        }

        //Drawing the GUI window
        private void OnGUI()
        {
            if (!showGUI || !Application.isPlaying)
                return;

            if (DontRecordInBuild)
                return;

            Rect windowRect = new Rect(0, 0, 256, 128);
            switch (guiPosition)
            {
                case GUIPositions.RightTop:
                    windowRect = new Rect(Screen.width - 256, 0, 256, 128);
                    break;

                case GUIPositions.RightBottom:
                    windowRect = new Rect(Screen.width - 256, Screen.height - 128, 256, 128);
                    break;

                case GUIPositions.LeftBottom:
                    windowRect = new Rect(0, Screen.height - 128, 256, 128);
                    break;

                case GUIPositions.Explicit:
                    windowRect = new Rect(explicitPosition.x, explicitPosition.y, 256, 128);
                    break;
            }
            GUI.Window(0, windowRect, Window, "Level Metrics");
        }
        #endregion

        #region Private methods
        private void Init()
        {
            for (int i = 0; i < recordables.Count; i++)
            {
                recordables[i].Init();
                runtimePaths.Add(recordables[i].ObjectToRecord.name, new Recordable.Runtime(recordables[i].Name, recordables[i].ObjectToRecord.name, recordables[i].Color));
            }

            foreach (Signal.Config signal in signals)
                signal.Init();
        }

        //Write all runtime data to the file
        private void Write()
        {
            if (DontRecordInBuild)
                return;

            bool writeComment = true;
            writeComment = WritePaths(writeComment);
            WriteSignals(!writeComment);

#if UNITY_EDITOR
            //Refresh the AssetDatabase to update file in the Unity.
            AssetDatabase.Refresh();
#endif
        }

        //Write paths data
        private bool WritePaths(bool writeComment)
        {
            if (runtimePaths.Count <= 0 && writeMode == WriteModes.Append)
                return false;

            foreach(KeyValuePair<string, Recordable.Runtime> pair in runtimePaths)
            {
                if (pair.Value.Data.Count <= 0)
                    continue;

                Recordable.Data data = pair.Value.Data[pair.Value.Data.Count - 1];
                data.isBreak = true;
            }

            string path = Utils.GetPathsFilePath();

            StreamWriter file;
            if (writeMode == WriteModes.Overwrite)
            {
                file = new StreamWriter(path);
            }
            else
            {
                file = new StreamWriter(path, true);
            }

            using (file)
            {
                if (writeComment)
                {
                    WriteComment(file);
                }

                foreach (KeyValuePair<string, Recordable.Runtime> pair in runtimePaths)
                {
                    if (pair.Value.Data.Count <= 0)
                        continue;

                    string lines = string.Format("PATH \"{0}:{1}\" ({2}):", pair.Key, pair.Value.name, '#' + ColorUtility.ToHtmlStringRGB(pair.Value.gizmoColor));
                    foreach (Recordable.Data data in pair.Value.Data)
                    {
                        string posString = string.Format("{0}, {1}, {2}", Utils.WriteFloat(data.pos.x), Utils.WriteFloat(data.pos.y), Utils.WriteFloat(data.pos.z));
                        lines += '\n' + string.Format("{0}: {1}, {2}", posString, Utils.WriteFloat(data.time), data.isBreak);
                    }
                    
                    file.WriteLine(lines);
                    file.WriteLine();
                }
            }

            return true;
        }

        //Write signals data
        private bool WriteSignals(bool writeComment)
        {
            if (runtimeSignals.Count <= 0 && writeMode == WriteModes.Append)
                return false;

            string path = Utils.GetSignalsFilePath();

            StreamWriter file;
            if (writeMode == WriteModes.Overwrite)
            {
                file = new StreamWriter(path);
            }
            else
            {
                file = new StreamWriter(path, true);
            }

            using (file)
            {
                if (writeComment)
                {
                    WriteComment(file);
                }

                foreach (KeyValuePair<string, List<Signal.Runtime>> pair in runtimeSignals)
                {
                    foreach(Signal.Runtime signal in pair.Value)
                    {
                        string lines = string.Format("SIGNAL \"{0}:{1}\" ({2}, {3}, {4}):", signal.objectName, signal.name, '#' + ColorUtility.ToHtmlStringRGB(signal.gizmoColor), Utils.WriteFloat(signal.gizmoSize), (int)signal.gizmoType);
                        bool write = false;
                        foreach (Signal.Data data in signal.Data)
                        {
                            if (data.count > signal.countThreshold)
                            {
                                write = true;
                                string posString = string.Format("{0}, {1}, {2}", Utils.WriteFloat(data.pos.x), Utils.WriteFloat(data.pos.y), Utils.WriteFloat(data.pos.z));
                                lines += '\n' + string.Format("{0}: {1}, {2}", posString, Utils.WriteFloat(data.time), data.count);
                            }
                        }
                        if (write)
                        {
                            file.WriteLine(lines);
                            file.WriteLine();
                        }
                    }
                }
            }

            return true;
        }

        //Write comment to the file
        private void WriteComment(StreamWriter file)
        {
            if (!string.IsNullOrEmpty(commentary))
            {
                string comment = "----COMMENTARY----\n" + commentary + "\n----COMMENTARY----\n";
                file.WriteLine(comment);
            }
        }

        //Process incoming signal
        private void ProcessSignal(Signal.Config signal, Vector3 pos)
        {
            if (!recording || pause || !signal.Record)
                return;

            if (DontRecordInBuild)
                return;

            if (signal.LookUpObject == null)
            {
                Debug.LogError(string.Format("Look up object of the signal {0} not found!", signal.Name));
                return;
            }

            if (runtimeSignals.ContainsKey(signal.LookUpObject.name))
            {
                Signal.Runtime targetSignal = runtimeSignals[signal.LookUpObject.name].FirstOrDefault(x => x.name == signal.Name);
                if (targetSignal == null)
                {
                    targetSignal = new Signal.Runtime(signal.Name, signal.LookUpObject.name, signal.CountThreshold, signal.GizmoColor, signal.GizmoSize, signal.GizmoType);
                    runtimeSignals[signal.LookUpObject.name].Add(targetSignal);
                }

                float minDist = 10000;
                Signal.Data targetData = null;
                foreach(Signal.Data data in targetSignal.Data)
                {
                    float dist = Vector3.Distance(pos, data.pos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetData = data;
                    }
                }

                if (minDist <= signal.DistanceThreshold)
                {
                    targetData.count++;
                }
                else
                {
                    targetSignal.Add(new Signal.Data(pos, 1, Time.timeSinceLevelLoad));
                }
            }
            else
            {
                runtimeSignals.Add(signal.LookUpObject.name, new List<Signal.Runtime>()
                {
                    { new Signal.Runtime(signal.Name, signal.LookUpObject.name, signal.CountThreshold, signal.GizmoColor, signal.GizmoSize, signal.GizmoType) }
                });
            }
        }

        //GUI Window function
        private void Window(int windowID)
        {
            if (recording)
            {
                if (pause)
                {
                    if (GUI.Button(new Rect(30, 41, 98, 46), "Continue"))
                    {
                        Continue();
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(30, 41, 98, 46), "Pause"))
                    {
                        Pause();
                    }
                }

                if (GUI.Button(new Rect(128, 41, 98, 46), "Stop"))
                {
                    Stop();
                }
            }
            else
            {
                if (GUI.Button(new Rect(30, 41, 196, 46), "Record"))
                {
                    Record();
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the recordable with given "name".
        /// </summary>
        public Recordable.Config GetRecordable(string name, string objectName)
        {
            return recordables.FirstOrDefault(x => x.Name == name && x.ObjectToRecord != null && x.ObjectToRecord.name == objectName);
        }

        /// <summary>
        /// Returns the signal with given "name".
        /// </summary>
        public Signal.Config GetSignal(string name, string objectName)
        {
            return signals.FirstOrDefault(x => x.Name == name && x.ObjectName == objectName);
        }

        /// <summary>
        /// Returns the recordable index with given "name".
        /// </summary>
        public int GetRecordableIndex(string name, string objectName)
        {
            Recordable.Config recordable = GetRecordable(name, objectName);
            if (recordable == null)
                return -1;
            return recordables.IndexOf(recordable);
        }

        /// <summary>
        /// Returns the signals index with given "name".
        /// </summary>
        public int GetSignalIndex(string name, string objectName)
        {
            Signal.Config signal = GetSignal(name, objectName);
            if (signal == null)
                return -1;
            return signals.IndexOf(signal);
        }

        /// <summary>
        /// Send signal.
        /// </summary>
        public void SendSignal(string id, string objectName)
        {
            int index = GetSignalIndex(id, objectName);
            if (index < 0)
            {
                Debug.LogError(string.Format("Signal with name \"{0}\" and object \"{1}\" not found!", id, objectName));
                return;
            }

            SendSignal(index);
        }

        /// <summary>
        /// Send signal.
        /// </summary>
        public void SendSignal(int id)
        {
            if (id < 0 || id >= signals.Count)
            {
                Debug.LogError(string.Format("Signal with index {0} not found!", id));
                return;
            }

            ProcessSignal(signals[id], signals[id].LookUpObject.position);
        }

        /// <summary>
        /// Record the data.
        /// </summary>
        public void Record()
        {
            recording = true;
        }

        /// <summary>
        /// Stop recording the data.
        /// </summary>
        public void Stop()
        {
            recording = false;
        }

        /// <summary>
        /// Pause the recording.
        /// </summary>
        public void Pause()
        {
            pause = true;
        }

        /// <summary>
        /// Continue the recording.
        /// </summary>
        public void Continue()
        {
            pause = false;
        }

        /// <summary>
        /// Add new signal config.
        /// </summary>
        /// <returns>Index of the new signal config. Return -1 if failed to add.</returns>
        public int AddSignal(Signal.Config config)
        {
            if (config.LookUpObject == null)
            {
                Debug.LogError("Signal doesnt have the look up object!");
                return -1;
            }

            config.Init();
            Signal.Config signal = signals.FirstOrDefault(x => x.ObjectName == config.ObjectName && x.Name == config.Name);
            if (signal != null)
            {
                int index = signals.IndexOf(signal);
                signals[index] = config;
                return index;
            }
            else
            {
                signals.Add(config);
                return signals.Count - 1;
            }
        }

        /// <summary>
        /// Add new recordable config.
        /// </summary>
        public void AddRecordable(Recordable.Config config)
        {
            if (config.ObjectToRecord == null)
            {
                Debug.LogError("Recordable doesnt have the object to record!");
                return;
            }

            config.Init();
            if (runtimePaths.ContainsKey(config.ObjectName))
            {
                int index = recordables.IndexOf(recordables.FirstOrDefault(x => x.ObjectName == config.ObjectName));
                recordables[index] = config;
            }
            else
            {
                recordables.Add(config);
                runtimePaths.Add(config.ObjectToRecord.name, new Recordable.Runtime(config.Name, config.ObjectToRecord.name, config.Color));
            }
        }
        #endregion

#if UNITY_EDITOR
        #region Editor methods
        /// <summary>
        /// Creates empty gameObject with LevelMetrics component.
        /// </summary>
        [MenuItem("Tools/LevelMetrics/Create LevelMetrics")]
        public static void CreateLevelMetrics()
        {
            GameObject go = new GameObject("LevelMetrics");
            go.AddComponent<LevelMetrics>();
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Creates empty gameObject with LevelMetricsViewer component.
        /// </summary>
        [MenuItem("Tools/LevelMetrics/Create LevelMetricsViewer")]
        public static void CreateLevelMetricsViewer()
        {
            GameObject go = new GameObject("LevelMetricsViewer");
            go.AddComponent<LevelMetricsViewer>();
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Open LevelMetrics documentation.
        /// </summary>
        [MenuItem("Tools/LevelMetrics/Open Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://paper.dropbox.com/doc/LevelMetricks-documentation--BBVh~h1ohhsEingGR9wLtMFoAQ-dCJqZOu0KyyLycrX7XQvS");
        }
        #endregion
#endif
    }
}