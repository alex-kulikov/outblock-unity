using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{

    [CustomEditor(typeof(UniversalTrigger))]
    public class UniversalTriggerEditor : Editor
    {

        private SerializedProperty onCollision;
        private SerializedProperty onEnter;
        private SerializedProperty tags;
        private SerializedProperty once;
        private SerializedProperty reloadTime;
        private SerializedProperty sound;

        private SerializedProperty sensorMode;
        private SerializedProperty handIK;

        private SerializedProperty conditionMode;
        private SerializedProperty item;
        private SerializedProperty requiredItemCount;

        private SerializedProperty actuationMode;
        private SerializedProperty switchMode;
        private SerializedProperty initState;
        private SerializedProperty basicActuator;
        private SerializedProperty switchStates;

        private bool sensorFoldOut = false;
        private bool conditionFoldOut = false;
        private bool actuationFoldOut = false;

        private void OnEnable()
        {
            onCollision = serializedObject.FindProperty("onCollision");
            onEnter = serializedObject.FindProperty("onEnter");
            tags = serializedObject.FindProperty("tags");
            once = serializedObject.FindProperty("once");
            reloadTime = serializedObject.FindProperty("reloadTime");
            sound = serializedObject.FindProperty("sound");

            sensorMode = serializedObject.FindProperty("sensorMode");
            handIK = serializedObject.FindProperty("handIK");

            conditionMode = serializedObject.FindProperty("conditionMode");
            item = serializedObject.FindProperty("item");
            requiredItemCount = serializedObject.FindProperty("requiredItemCount");

            actuationMode = serializedObject.FindProperty("actuationMode");
            switchMode = serializedObject.FindProperty("switchMode");
            initState = serializedObject.FindProperty("initState");
            basicActuator = serializedObject.FindProperty("basicActuator");
            switchStates = serializedObject.FindProperty("switchStates");
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable | GizmoType.NotInSelectionHierarchy)]
        private static void DrawGizmo(UniversalTrigger src, GizmoType gizmoType)
        {
            if (!InternalEditorUtility.GetIsInspectorExpanded(src))
                return;

            if (!Trigger.ShowLines)
                return;

            if (src.ActuationMode == UniversalTrigger.ActuationModes.Basic)
            {
                foreach (Trigger trigger in src.BasicActuator.Triggers)
                {
                    if (trigger != null)
                    {
                        UtilsEditor.DrawArrowLine(src.transform.position, trigger.transform.position);
                    }
                }

                UtilsEditor.DrawEventLine(src.transform, src.BasicActuator.OnTrigger);
            }
            else
            {
                foreach (UniversalTrigger.Actuator state in src.SwitchStates)
                {
                    foreach (Trigger trigger in state.Triggers)
                    {
                        if (trigger != null)
                        {
                            UtilsEditor.DrawArrowLine(src.transform.position, trigger.transform.position);
                        }
                    }

                    UtilsEditor.DrawEventLine(src.transform, state.OnTrigger);
                }
            }
        }

        private void FoldOutHelpBox(ref bool foldOut, string label, System.Action content)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            foldOut = EditorGUILayout.Foldout(foldOut, label);
            if (foldOut)
            {
                EditorGUILayout.Space();
                content.Invoke();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(onCollision);
            EditorGUILayout.PropertyField(onEnter);
            EditorGUILayout.PropertyField(tags, true);
            EditorGUILayout.PropertyField(once);
            EditorGUILayout.PropertyField(reloadTime);
            EditorGUILayout.PropertyField(sound);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Universal Trigger", EditorStyles.boldLabel);

            FoldOutHelpBox(ref sensorFoldOut, "Sensor", () =>
            {
                EditorGUILayout.PropertyField(sensorMode);
                if (sensorMode.enumValueIndex > 0)
                {
                    EditorGUILayout.PropertyField(handIK);
                }
            });

            EditorGUILayout.Space();

            FoldOutHelpBox(ref conditionFoldOut, "Condition", () =>
            {
                EditorGUILayout.PropertyField(conditionMode);
                if (conditionMode.enumValueIndex > 0)
                {
                    EditorGUILayout.PropertyField(item);
                    EditorGUILayout.PropertyField(requiredItemCount);
                }
            });

            EditorGUILayout.Space();

            FoldOutHelpBox(ref actuationFoldOut, "Actuation", () =>
            {
                EditorGUILayout.PropertyField(actuationMode);
                if (actuationMode.enumValueIndex == 0)
                {
                    EditorGUILayout.PropertyField(basicActuator.FindPropertyRelative("triggers"));
                    EditorGUILayout.PropertyField(basicActuator.FindPropertyRelative("onTrigger"));
                }
                else
                {
                    EditorGUILayout.PropertyField(switchMode);
                    EditorGUILayout.PropertyField(initState);

                    EditorGUILayout.Space();

                    for (int i = 0; i < switchStates.arraySize; i++)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("State " + i, EditorStyles.boldLabel);

                        GUI.enabled = i > 0;
                        if (GUILayout.Button("▲", GUILayout.MaxWidth(20)))
                        {
                            switchStates.MoveArrayElement(i, i - 1);
                            break;
                        }
                        GUI.enabled = i < switchStates.arraySize - 1;
                        if (GUILayout.Button("▼", GUILayout.MaxWidth(20)))
                        {
                            switchStates.MoveArrayElement(i, i + 1);
                            break;
                        }
                        GUI.enabled = switchStates.arraySize > 2;
                        if (GUILayout.Button("X", GUILayout.MaxWidth(32)))
                        {
                            switchStates.DeleteArrayElementAtIndex(i);
                            break;
                        }
                        GUI.enabled = true;

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(switchStates.GetArrayElementAtIndex(i).FindPropertyRelative("triggers"));
                        EditorGUILayout.PropertyField(switchStates.GetArrayElementAtIndex(i).FindPropertyRelative("onTrigger"));
                        EditorGUILayout.EndVertical();
                    }

                    if (GUILayout.Button("Add"))
                    {
                        switchStates.InsertArrayElementAtIndex(switchStates.arraySize);
                    }
                }
            });

            serializedObject.ApplyModifiedProperties();
        }

    }
}