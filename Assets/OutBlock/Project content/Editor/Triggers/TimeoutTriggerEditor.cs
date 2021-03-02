using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{

    [CustomEditor(typeof(TimeoutTrigger))]
    public class TimeoutTriggerEditor : Editor
    {

        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.NonSelected)]
        private static void DrawGizmo(TimeoutTrigger src, GizmoType gizmoType)
        {
            if (!InternalEditorUtility.GetIsInspectorExpanded(src))
                return;

            if (!Trigger.ShowLines)
                return;

            UtilsEditor.DrawEventLine(src.transform, src.TimeOutEvent);
        }

    }
}