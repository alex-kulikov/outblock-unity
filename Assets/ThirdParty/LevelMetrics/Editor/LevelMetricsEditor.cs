using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Denver.Metrics
{

    /// <summary>
    /// Custom editor class for the LevelMetrics class
    /// </summary>
    [CustomEditor(typeof(LevelMetrics))]
    public class LevelMetricsEditor : Editor
    {

        private SerializedProperty recording;
        private SerializedProperty showGUI;
        private SerializedProperty guiPosition;
        private SerializedProperty explicitPosition;
        private SerializedProperty signals;
        private SerializedProperty recordables;
        private SerializedProperty writeMode;
        private SerializedProperty recordOnBuild;
        private SerializedProperty commentary;

        private void OnEnable()
        {
            //load all properties from the serialized object
            recording = serializedObject.FindProperty("recording");
            showGUI = serializedObject.FindProperty("showGUI");
            guiPosition = serializedObject.FindProperty("guiPosition");
            explicitPosition = serializedObject.FindProperty("explicitPosition");
            signals = serializedObject.FindProperty("signals");
            recordables = serializedObject.FindProperty("recordables");
            writeMode = serializedObject.FindProperty("writeMode");
            recordOnBuild = serializedObject.FindProperty("recordOnBuild");
            commentary = serializedObject.FindProperty("commentary");
        }

        /// <summary>
        /// Method to draw any array more beatiful way
        /// </summary>
        /// <param name="property">Array property</param>
        /// <param name="name">Array display name</param>
        /// <param name="elementBody">Array element body content</param>
        private void DrawArray(SerializedProperty property, string name, System.Action<SerializedProperty> elementBody)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, name, style);
            style.fontStyle = previousStyle;

            if (!property.isExpanded)
                return;

            int indend = EditorGUI.indentLevel;

            EditorGUI.indentLevel++;
            property.arraySize = EditorGUILayout.IntField("Size", property.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty arrayElement = property.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                arrayElement.isExpanded = EditorGUILayout.Foldout(arrayElement.isExpanded, arrayElement.FindPropertyRelative("name").stringValue);
                if (GUILayout.Button("X", GUILayout.MaxWidth(48)))
                {
                    property.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                if (arrayElement.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    elementBody.Invoke(arrayElement);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel = indend;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
                property.InsertArrayElementAtIndex(property.arraySize);
            if (GUILayout.Button("Clear"))
                property.ClearArray();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Method to draw an signal array.
        /// </summary>
        /// <param name="property">Array property(must be as LevelMetrics.Signal class)</param>
        private void DrawSignals(SerializedProperty property)
        {
            DrawArray(property, "Signals", (arrayElement) =>
            {
                EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("name"));
                SerializedProperty lookUpObject = arrayElement.FindPropertyRelative("lookUpObject");
                EditorGUILayout.PropertyField(lookUpObject);
                if (lookUpObject.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("record"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("countThreshold"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("distanceThreshold"));

                    SerializedProperty activateOnKey = arrayElement.FindPropertyRelative("activateOnKey");
                    EditorGUILayout.PropertyField(activateOnKey);
                    if (activateOnKey.boolValue)
                    {
                        EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("button"));
                        EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("key"));
                    }

                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("gizmoColor"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("gizmoSize"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("gizmoType"));
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign the LookUpObject to this signal to work!", MessageType.Error);
                }
                EditorGUILayout.Space();
            });
        }

        /// <summary>
        /// Method to draw an path array.
        /// </summary>
        /// <param name="property">Array property(must be as LevelMetrics.Recordable class)</param>
        private void DrawRecordables(SerializedProperty property)
        {
            DrawArray(property, "Recordables", (arrayElement) =>
            {
                EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("name"));
                SerializedProperty objectToRecord = arrayElement.FindPropertyRelative("objectToRecord");
                EditorGUILayout.PropertyField(objectToRecord);
                if (objectToRecord.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("record"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("delay"));
                    EditorGUILayout.PropertyField(arrayElement.FindPropertyRelative("color"));
                }
                EditorGUILayout.Space();
            });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(recording);
            EditorGUILayout.PropertyField(showGUI);
            if (showGUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(guiPosition);
                if (guiPosition.enumValueIndex == 4)
                    EditorGUILayout.PropertyField(explicitPosition);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            GUI.enabled = !Application.isPlaying;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            DrawSignals(signals);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            DrawRecordables(recordables);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            GUI.enabled = true;

            EditorGUILayout.PropertyField(writeMode);
            EditorGUILayout.PropertyField(recordOnBuild);
            if (recordOnBuild.boolValue)
                EditorGUILayout.HelpBox("Record files will be in the data folder of the application.", MessageType.Info);
            EditorGUILayout.PropertyField(commentary);

            serializedObject.ApplyModifiedProperties();
        }

    }
}