using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{
    [CustomEditor(typeof(ActivationTrigger))]
    public class ActivationTriggerEditor : Editor
    {

        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.NonSelected)]
        private static void DrawGizmo(ActivationTrigger src, GizmoType gizmoType)
        {
            if (!InternalEditorUtility.GetIsInspectorExpanded(src))
                return;

            if (!Trigger.ShowLines)
                return;

            foreach (GameObject go in src.Objects)
            {
                if (go != null)
                {
                    UtilsEditor.DrawArrowLine(src.transform.position, go.transform.position);
                }
            }
        }

    }
}