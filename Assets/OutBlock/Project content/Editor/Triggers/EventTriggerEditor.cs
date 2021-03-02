using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditorInternal;

namespace OutBlock
{

    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerEditor : Editor
    {

        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.NonSelected)]
        private static void DrawGizmo(EventTrigger src, GizmoType gizmoType)
        {
            if (!InternalEditorUtility.GetIsInspectorExpanded(src))
                return;

            if (!Trigger.ShowLines)
                return;

            UtilsEditor.DrawEventLine(src.transform, src.OnTrigger);
        }

    }
}