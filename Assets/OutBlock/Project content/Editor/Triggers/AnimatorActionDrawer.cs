using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OutBlock
{

    [CustomPropertyDrawer(typeof(AnimatorTrigger.AnimatorAction))]
    public class AnimatorActionDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty type = property.FindPropertyRelative("actionType");
            int count;
            switch (type.enumValueIndex)
            {
                case 2:
                    count = 3;
                    break;

                case 3:
                    count = 4;
                    break;

                case 4:
                    count = 3;
                    break;

                case 5:
                    count = 5;
                    break;

                default:
                    count = 2;
                    break;
            }

            return EditorGUIUtility.singleLineHeight * (property.isExpanded ? count : 1) + 6;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.HelpBox(position, "", MessageType.None);

            Rect propertyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(propertyRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                Rect typeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width - 16, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("actionType"));
                SerializedProperty type = property.FindPropertyRelative("actionType");

                if (type.enumValueIndex > 1 && type.enumValueIndex <= 3)
                {
                    Rect stateRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width - 16, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(stateRect, property.FindPropertyRelative("stateName"));

                    if (type.enumValueIndex == 3)
                    {
                        Rect timeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3, position.width - 16, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PropertyField(timeRect, property.FindPropertyRelative("transitionTime"));
                    }
                }
                else if (type.enumValueIndex == 4)
                {
                    Rect speedRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width - 16, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(speedRect, property.FindPropertyRelative("speed"));
                }
                else if (type.enumValueIndex > 4)
                {
                    Rect parameterTypeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width - 16, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(parameterTypeRect, property.FindPropertyRelative("parameterType"));

                    Rect parameterRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3, position.width - 16, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(parameterRect, property.FindPropertyRelative("parameterName"));

                    SerializedProperty parameterType = property.FindPropertyRelative("parameterType");
                    if (parameterType.enumValueIndex < 3)
                    {
                        string propertyName = "";
                        switch (parameterType.enumValueIndex)
                        {
                            case 0:
                                propertyName = "valueInt";
                                break;

                            case 1:
                                propertyName = "valueFloat";
                                break;

                            case 2:
                                propertyName = "valueBool";
                                break;
                        }

                        Rect valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 4, position.width - 16, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative(propertyName));
                    }
                }
            }

            EditorGUI.EndProperty();
        }

    }
}