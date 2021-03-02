using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{
    [CustomEditor(typeof(AIPath))]
    public class AIPathEditor : Editor
    {

        private SerializedProperty pathNodes;
        private SerializedProperty behaviour;
        private SerializedProperty nextPath;

        private bool editNodes;
        private LayerMask layerMask;
        private AIPath script;

        private void OnEnable()
        {
            pathNodes = serializedObject.FindProperty("pathNodes");
            behaviour = serializedObject.FindProperty("behaviour");
            nextPath = serializedObject.FindProperty("nextPath");
            script = (AIPath)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            editNodes = EditorGUILayout.Toggle("Edit nodes", editNodes);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(behaviour);
            if (behaviour.enumValueIndex == 3)
                EditorGUILayout.PropertyField(nextPath);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(pathNodes, true);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                Undo.RecordObject(script, "Added new point");
                script.PathNodes.Add(new AIPath.PathNode(script.PathNodes[script.PathNodes.Count - 1].Pos + Vector3.forward, script.PathNodes[script.PathNodes.Count - 1].Dir, 0));
            }

            if (script.PathNodes.Count <= 0)
                GUI.enabled = false;
            if (GUILayout.Button("Remove"))
            {
                Undo.RecordObject(script, "Removed last point");
                script.PathNodes.RemoveAt(script.PathNodes.Count - 1);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(script, "Clear points");
                script.PathNodes.Clear();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Align to the ground"))
            {
                for (int i = 0; i < script.PathNodes.Count; i++)
                {
                    Ray ray = new Ray(script.PathNodes[i].Pos, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
                    {
                        Undo.RecordObject(script, "Point aligned to the ground");
                        script.PathNodes[i].Pos = script.transform.InverseTransformPoint(hit.point);
                    }
                }
            }

            LayerMask tempMask = EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            if (script.PathNodes == null || script.PathNodes.Count <= 0)
                return;

            SceneView.RepaintAll();

            for (int i = 0; i < script.PathNodes.Count; i++)
            {
                Vector3 pos = script.transform.TransformPoint(script.PathNodes[i].Pos);
                Quaternion rot = Quaternion.LookRotation(script.transform.TransformDirection(script.PathNodes[i].Dir), Vector3.up);

                Handles.color = Handles.zAxisColor;
                Handles.ArrowHandleCap(0, pos, rot, 1, EventType.Repaint);

                Handles.color = Color.white;
                Handles.Label(pos, i.ToString());
            }

            if (!editNodes)
                return;

            EditorGUI.BeginChangeCheck();

            Vector3[] newPoses = new Vector3[script.PathNodes.Count];
            Quaternion[] newDirs = new Quaternion[script.PathNodes.Count];

            for (int i = 0; i < script.PathNodes.Count; i++)
            {
                Vector3 pos = script.transform.TransformPoint(script.PathNodes[i].Pos);
                Quaternion rot = Quaternion.LookRotation(script.transform.TransformDirection(script.PathNodes[i].Dir), Vector3.up);

                newPoses[i] = pos;
                newDirs[i] = rot;
                if (Tools.current == Tool.Move)
                {
                    newPoses[i] = Handles.PositionHandle(pos, Quaternion.identity);
                }
                else if (Tools.current == Tool.Rotate)
                {
                    newDirs[i] = Handles.RotationHandle(rot, pos);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < script.PathNodes.Count; i++)
                {
                    Undo.RecordObject(script, "Change path node");
                    script.PathNodes[i].Pos = script.transform.InverseTransformPoint(newPoses[i]);
                    script.PathNodes[i].Dir = script.transform.InverseTransformDirection(newDirs[i] * Vector3.forward);
                }
            }
        }

    }
}