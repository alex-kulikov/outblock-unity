using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Denver.Metrics
{

    /// <summary>
    /// Component for the viewing LevelMetrics files.
    /// </summary>
    public class LevelMetricsViewer : MonoBehaviour
    {

        [SerializeField, Space, Tooltip("Display gizmos?")]
        private bool display = true;
        /// <summary>
        /// Display gizmos?
        /// </summary>
        public bool Display => display;
        [SerializeField, Tooltip("Data file for the signals.")]
        private TextAsset signalsFile = null;
        [SerializeField]
        private TextAsset pathsFile = null;

        private TextAsset oldSignalFile = null;
        private TextAsset oldPathFile = null;

        private string comment = "";
        public string Comment => comment;

        public Dictionary<string, List<Signal.Runtime>> editorSignals { get; private set; } = new Dictionary<string, List<Signal.Runtime>>();
        public Dictionary<string, Recordable.Runtime> editorPaths { get; private set; } = new Dictionary<string, Recordable.Runtime>();

        public delegate void OnFilesReread();
        public OnFilesReread onFilesReread;

        private const string nameRegex = "\"([^\"]*)\"";
        private const string parenthesesRegex = @"\(([^)]+)\)";

#if UNITY_EDITOR

        //If the display file has changed then reread the file.
        private void OnValidate()
        {
            if (oldSignalFile != signalsFile || oldPathFile != pathsFile || editorSignals.Count == 0)
            {
                oldSignalFile = signalsFile;
                oldPathFile = pathsFile;
                RereadFiles();
            }
        }

        //Drawing the signals
        private void OnDrawGizmosSelected()
        {
            if (!display)
                return;

            if (editorSignals.Count > 0)
            {
                foreach (KeyValuePair<string, List<Signal.Runtime>> pair in editorSignals)
                {
                    foreach(Signal.Runtime signal in pair.Value)
                    {
                        if (!signal.treeElement.IsVisible())
                            continue;

                        foreach (Signal.Data data in signal.Data)
                        {
                            if (data.treeElement.IsVisible())
                                Utils.DrawGizmo(data.pos, signal.gizmoColor, signal.gizmoType, signal.gizmoSize);
                        }
                    }
                }
            }

            if (editorPaths.Count > 0)
            {
                foreach (KeyValuePair<string, Recordable.Runtime> pair in editorPaths)
                {
                    if (!pair.Value.treeElement.IsVisible())
                        continue;

                    Utils.DrawPath(Recordable.Data.ToVector3(pair.Value.Data), pair.Value.gizmoColor);
                }
            }
        }

        //Read signals file
        private void ReadSignals()
        {
            editorSignals = new Dictionary<string, List<Signal.Runtime>>();

            if (signalsFile == null || string.IsNullOrEmpty(signalsFile.text))
                return;

            string[] lines = signalsFile.text.Split('\n');

            if (lines.Length < 2)
                return;

            Signal.Runtime currentSignal = null;
            bool commentary = false;
            comment = "";
            foreach (string line in lines)
            {
                ReadComment(line, ref commentary, ref comment);

                if (currentSignal == null)
                {
                    if (line.Contains("SIGNAL"))
                    {
                        string[] signalInfo = Regex.Match(line, nameRegex).ToString().Trim('"').Split(':');
                        string lineNext = line.Substring(line.LastIndexOf('"'));
                        string[] displayInfo = Regex.Match(lineNext, parenthesesRegex).ToString().Trim('(', ')').Split(',');
                        ColorUtility.TryParseHtmlString(displayInfo[0], out Color color);
                        currentSignal = new Signal.Runtime(signalInfo[1], signalInfo[0], 0, color, Utils.ReadFloat(displayInfo[1]), (GizmoTypes)int.Parse(displayInfo[2]));
                    }
                }
                else
                {
                    string[] sides = line.Split(':');

                    if (sides.Length > 0 && line.Length > 3)
                    {
                        string[] posString = sides[0].Split(',');
                        Vector3 pos = new Vector3(Utils.ReadFloat(posString[0]), Utils.ReadFloat(posString[1]), Utils.ReadFloat(posString[2]));
                        string[] dataString = sides[1].Split(',');
                        float time = Utils.ReadFloat(dataString[0]);
                        int count = int.Parse(dataString[1]);

                        if (editorSignals.ContainsKey(currentSignal.objectName))
                        {
                            Signal.Runtime signal = editorSignals[currentSignal.objectName].FirstOrDefault(x => x.name == currentSignal.name);
                            if (signal == null)
                            {
                                signal = currentSignal;
                                editorSignals[currentSignal.objectName].Add(currentSignal);
                            }
                            signal.Add(new Signal.Data(pos, count, time));
                        }
                        else
                        {
                            currentSignal.Add(new Signal.Data(pos, count, time));
                            editorSignals.Add(currentSignal.objectName, new List<Signal.Runtime>()
                            {
                                { currentSignal }
                            });
                        }
                    }
                    else
                    {
                        currentSignal = null;
                    }
                }
            }
        }

        //Read paths file
        private void ReadPaths()
        {
            editorPaths = new Dictionary<string, Recordable.Runtime>();

            if (pathsFile == null || string.IsNullOrEmpty(pathsFile.text))
                return;

            string[] lines = pathsFile.text.Split('\n');

            if (lines.Length < 2)
                return;

            Recordable.Runtime currentPath = null;
            bool commentary = false;
            comment = "";
            foreach (string line in lines)
            {
                ReadComment(line, ref commentary, ref comment);

                if (currentPath == null)
                {
                    if (line.Contains("PATH"))
                    {
                        string[] pathInfo = Regex.Match(line, nameRegex).ToString().Trim('"').Split(':');
                        string lineNext = line.Substring(line.LastIndexOf('"'));
                        string colorInfo = Regex.Match(lineNext, parenthesesRegex).ToString().Trim('(', ')');
                        ColorUtility.TryParseHtmlString(colorInfo, out Color color);
                        currentPath = new Recordable.Runtime(pathInfo[1], pathInfo[0], color);
                    }
                }
                else
                {
                    string[] sides = line.Split(':');

                    if (sides.Length > 0 && line.Length > 3)
                    {
                        string[] posString = sides[0].Split(',');
                        Vector3 pos = new Vector3(Utils.ReadFloat(posString[0]), Utils.ReadFloat(posString[1]), Utils.ReadFloat(posString[2]));
                        string[] dataString = sides[1].Split(',');
                        float time = Utils.ReadFloat(dataString[0]);
                        bool isBreak = bool.Parse(dataString[1]);

                        if (editorPaths.ContainsKey(currentPath.objectName))
                        {
                            editorPaths[currentPath.objectName].Add(new Recordable.Data(time, pos, isBreak));
                        }
                        else
                        {
                            currentPath.Add(new Recordable.Data(time, pos, isBreak));
                            editorPaths.Add(currentPath.objectName, currentPath);
                        }
                    }
                    else
                    {
                        currentPath = null;
                    }
                }
            }
        }

        private void ReadComment(string line, ref bool commentary, ref string comment)
        {
            if (!commentary)
            {
                if (line.Contains("COMMENTARY"))
                {
                    commentary = true;
                }
            }
            else
            {
                if (line.Contains("COMMENTARY"))
                {
                    commentary = false;
                }
                else
                {
                    comment += line;
                }
            }
        }

        /// <summary>
        /// Reread display files.
        /// </summary>
        public void RereadFiles()
        {
            if (Application.isPlaying)
                return;

            ReadSignals();
            ReadPaths();

            onFilesReread?.Invoke();
        }

#endif

    }
}