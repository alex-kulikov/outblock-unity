using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OutBlock
{
    [CustomEditor(typeof(Moveable))]
    public class MoveableEditor : Editor
    {

        private SerializedProperty movementType;
        private SerializedProperty startPos;
        private SerializedProperty endPos;
        private SerializedProperty startRot;
        private SerializedProperty endRot;
        private SerializedProperty spline;
        private SerializedProperty mode;
        private SerializedProperty waitTime;
        private SerializedProperty moveTime;
        private SerializedProperty ticks;
        private SerializedProperty events;
        private Moveable script;

        private void OnEnable()
        {
            movementType = serializedObject.FindProperty("movementType");
            startPos = serializedObject.FindProperty("startPos");
            endPos = serializedObject.FindProperty("endPos");
            startRot = serializedObject.FindProperty("startRot");
            endRot = serializedObject.FindProperty("endRot");
            waitTime = serializedObject.FindProperty("waitTime");
            moveTime = serializedObject.FindProperty("moveTime");
            spline = serializedObject.FindProperty("spline");
            mode = serializedObject.FindProperty("mode");
            ticks = serializedObject.FindProperty("ticks");
            events = serializedObject.FindProperty("events");
            script = (Moveable)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(movementType);

            if (movementType.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(startPos);
                EditorGUILayout.PropertyField(startRot);
                EditorGUILayout.PropertyField(endPos);
                EditorGUILayout.PropertyField(endRot);
            }
            else
            {
                EditorGUILayout.PropertyField(spline);
                if (spline.objectReferenceValue == null)
                {
                    if (GUILayout.Button("Add new"))
                    {
                        GameObject go = new GameObject("Spline");
                        go.transform.position = script.transform.position;
                        spline.objectReferenceValue = go.AddComponent<BezierSpline>();
                    }
                }
                EditorGUILayout.PropertyField(mode);
            }

            EditorGUILayout.PropertyField(waitTime);
            EditorGUILayout.PropertyField(moveTime);
            EditorGUILayout.PropertyField(ticks);
            EditorGUILayout.PropertyField(events, true);

            EditorGUILayout.Space();

            if (movementType.enumValueIndex == 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set start pos"))
                {
                    startPos.vector3Value = script.transform.position;
                }
                if (GUILayout.Button("Set end pos"))
                {
                    endPos.vector3Value = script.transform.position;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set start rot"))
                {
                    startRot.vector3Value = script.transform.eulerAngles;
                }
                if (GUILayout.Button("Set end rot"))
                {
                    endRot.vector3Value = script.transform.eulerAngles;
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}