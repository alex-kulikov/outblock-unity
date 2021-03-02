using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MetricCalculator : EditorWindow
{

    private float speed = 3.5f;
    private float inputGravity = 3;
    private float minValue = 0.3f;
    private float distance = 5;
    private bool showInternal;

    [MenuItem("OutBlock/Metric Calculator")]
    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(MetricCalculator));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Character speed", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Options:", EditorStyles.boldLabel);
        speed = EditorGUILayout.FloatField("Speed: ", speed);
        distance = EditorGUILayout.FloatField("Distance: ", distance);
        showInternal = EditorGUILayout.Toggle("Show internal parameters", showInternal);
        if (showInternal)
        {
            EditorGUILayout.HelpBox("Be aware that these values are hardcoded and you should NOT change them!", MessageType.Warning);
            inputGravity = EditorGUILayout.FloatField("Input Gravity:", inputGravity);
            minValue = EditorGUILayout.FloatField("Min Value:", minValue);
            EditorGUILayout.EndVertical();
        }

        float dt = 1f / inputGravity;
        float dv = speed - minValue;
        float ds = dv * dt / 2f;
        float time = (distance - ds) / speed + dt;
        EditorGUILayout.LabelField(string.Format("Results: {0}seconds", time), EditorStyles.boldLabel);

        EditorGUILayout.EndVertical();
    }

}