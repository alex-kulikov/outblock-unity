using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Denver.Metrics
{

    /// <summary>
    /// Custom editor class for the LevelMetrics class
    /// </summary>
    [CustomEditor(typeof(LevelMetricsViewer))]
    public class LevelMetricsViewerEditor : Editor
    {

        [SerializeField]
        private TreeViewState treeViewState;

        private LevelMetricsTreeView tree;

        private SerializedProperty display;
        private SerializedProperty displaySettings;
        private SerializedProperty signalsFile;
        private SerializedProperty pathsFile;

        private LevelMetricsViewer script;

        private bool commentFoldOut = true;
        private bool hierarchyFoldOut = true;

        private void OnEnable()
        {
            display = serializedObject.FindProperty("display");
            displaySettings = serializedObject.FindProperty("displaySettings");
            signalsFile = serializedObject.FindProperty("signalsFile");
            pathsFile = serializedObject.FindProperty("pathsFile");

            script = (LevelMetricsViewer)target;
            script.onFilesReread += ReloadTree;

            if (treeViewState == null)
                treeViewState = new TreeViewState();

            ReloadTree();
        }

        private void OnDisable()
        {
            script.onFilesReread -= ReloadTree;
        }

        private void ReloadTree()
        {
            List<LevelMetricsTreeElement> elements = CollectTree();
            tree = new LevelMetricsTreeView(treeViewState, elements);
            //tree.ExpandAll();
        }

        private List<LevelMetricsTreeElement> CollectTree()
        {
            List<LevelMetricsTreeElement> result = new List<LevelMetricsTreeElement>();

            LevelMetricsTreeElement root = new LevelMetricsTreeElement() { id = 0, depth = -1, displayName = "root" };
            LevelMetricsTreeElement signals = new LevelMetricsTreeElement() { id = 1, displayName = "Signals" };
            LevelMetricsTreeElement paths = new LevelMetricsTreeElement() { id = 2, displayName = "Paths" };

            root.AddChild(signals);
            root.AddChild(paths);

            result.Add(root);
            result.Add(signals);
            result.Add(paths);

            int id = 3;
            for (int i = 0; i < script.editorSignals.Count; i++)
            {
                int objectIndex;
                List<Signal.Runtime> signalList = script.editorSignals.ElementAt(i).Value;
                foreach(Signal.Runtime signal in signalList)
                {
                    if (!CheckElementInBranch(signal.objectName, result, out objectIndex))
                    {
                        LevelMetricsTreeElement objectRoot = new LevelMetricsTreeElement();
                        objectRoot.id = id++;
                        objectRoot.displayName = signal.objectName;
                        signals.AddChild(objectRoot);

                        result.Add(objectRoot);
                        objectIndex = result.Count - 1;
                    }

                    signal.treeElement.Reset();
                    signal.treeElement.displayName = signal.name;
                    signal.treeElement.id = id++;
                    result[objectIndex].AddChild(signal.treeElement);
                    result.Add(signal.treeElement);

                    for (int d = 0; d < signal.Data.Count; d++)
                    {
                        signal.Data[d].treeElement.Reset();
                        signal.Data[d].treeElement.displayName = string.Format("{0}: {1} at {2}", signal.name, signal.Data[d].count, Utils.FloatToTime(signal.Data[d].time));
                        signal.Data[d].treeElement.id = id++;
                        signal.treeElement.AddChild(signal.Data[d].treeElement);

                        result.Add(signal.Data[d].treeElement);
                    }
                }
            }

            for (int i = 0; i < script.editorPaths.Count; i++)
            {
                int objectIndex;
                Recordable.Runtime path = script.editorPaths.ElementAt(i).Value;
                if (!CheckElementInBranch(path.objectName, result.Where(x => !CheckParent("Signals", x)).ToList(), out objectIndex))
                {
                    LevelMetricsTreeElement objectRoot = new LevelMetricsTreeElement();
                    objectRoot.id = id++;
                    objectRoot.displayName = path.objectName;
                    paths.AddChild(objectRoot);

                    result.Add(objectRoot);
                    objectIndex = result.Count - 1;
                }

                path.treeElement.Reset();
                path.treeElement.displayName = path.name;
                path.treeElement.id = id++;
                result[objectIndex].AddChild(path.treeElement);
                result.Add(path.treeElement);
            }

            return result;
        }

        private bool CheckParent(string parentName, TreeViewItem item)
        {
            if (item.parent != null)
            {
                if (item.parent.displayName == parentName)
                    return true;
                else 
                    return CheckParent(parentName, item.parent);
            }
            else return false;
        }

        private bool CheckElementInBranch(string objectName, List<LevelMetricsTreeElement> elements, out int index)
        {
            index = -1;
            LevelMetricsTreeElement element = elements.FirstOrDefault(x => x.displayName == objectName);
            if (element != null)
            {
                index = elements.IndexOf(element);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draw labels on the signals gizmos.
        /// </summary>
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        private static void DrawHandles(LevelMetricsViewer scr, GizmoType gizmoType)
        {
            if (!scr.Display)
                return;

            if (!scr.DisplaySettings.ShowText)
                return;

            if (!Selection.Contains(scr.gameObject))
                return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            Bounds testBound = new Bounds(Vector3.zero, Vector3.one * 0.2f);

            if (scr.editorSignals.Count > 0)
            {
                foreach (KeyValuePair<string, List<Signal.Runtime>> pair in scr.editorSignals)
                {
                    foreach(Signal.Runtime signal in pair.Value)
                    {
                        string signalName = signal.name;

                        if (!signal.treeElement.IsVisible())
                            continue;

                        foreach (Signal.Data data in signal.Data)
                        {
                            testBound.center = data.pos;
                            if (scr.DisplaySettings.Mode == DisplaySettings.Modes.ViewCheck && !GeometryUtility.TestPlanesAABB(scr.Planes, testBound))
                                continue;

                            Vector3 pos = data.pos + Camera.current.transform.right * signal.gizmoSize * 0.5f;

                            if (data.treeElement.IsVisible())
                                Handles.Label(pos, string.Format("{0}: {1} at {2}", signalName, data.count, Utils.FloatToTime(data.time)), style);
                        }
                    }
                    
                }
            }

            if (scr.editorPaths.Count > 0)
            {
                foreach (KeyValuePair<string, Recordable.Runtime> pair in scr.editorPaths)
                {
                    Recordable.Runtime path = pair.Value;

                    if (!path.treeElement.IsVisible())
                        continue;

                    for (int i = 0; i < path.Data.Count; i++)
                    {
                        testBound.center = path.Data[i].pos;
                        if (!GeometryUtility.TestPlanesAABB(scr.Planes, testBound))
                            continue;

                        Vector3 pos = path.Data[i].pos + Camera.current.transform.right * 0.2f;
                        if (i == 0)
                            Handles.Label(pos, string.Format("{0}: {1}", path.name, Utils.FloatToTime(path.Data[i].time)), style);
                        else
                            Handles.Label(pos, Utils.FloatToTime(path.Data[i].time), style);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(signalsFile);
            if (GUILayout.Button("Find", GUILayout.MaxWidth(64)))
            {
                signalsFile.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(Utils.GetSignalsFilePath());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pathsFile);
            if (GUILayout.Button("Find", GUILayout.MaxWidth(64)))
            {
                pathsFile.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(Utils.GetPathsFilePath());
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Reveal directory"))
            {
                EditorUtility.RevealInFinder(Utils.GetDirectoryPath());
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(display);
            if (display.boolValue)
                EditorGUILayout.PropertyField(displaySettings, true);
            serializedObject.ApplyModifiedProperties();

            if (display.boolValue)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                GUIStyle style = EditorStyles.foldout;
                FontStyle previousStyle = style.fontStyle;
                style.fontStyle = FontStyle.Bold;
                hierarchyFoldOut = EditorGUILayout.Foldout(hierarchyFoldOut, "Hierarchy");
                style.fontStyle = previousStyle;

                if (hierarchyFoldOut)
                    tree.OnGUI(EditorGUILayout.GetControlRect(false, tree.totalHeight));

                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("Double click to focus on the object", MessageType.Info);
                EditorGUI.indentLevel++;
            }

            EditorGUILayout.Space();

            commentFoldOut = EditorGUILayout.Foldout(commentFoldOut, "Commentary");
            if (commentFoldOut)
            {
                EditorGUILayout.TextArea(script.Comment);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            if (GUILayout.Button("Reread files"))
            {
                script.RereadFiles();
            }
        }

    }

}