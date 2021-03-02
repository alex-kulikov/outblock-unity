using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{
    [CustomEditor(typeof(InteractionTrigger))]
    public class InteractionTriggerEditor : Editor
    {

        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.NonSelected)]
        private static void DrawGizmo(InteractionTrigger src, GizmoType gizmoType)
        {
            if (!InternalEditorUtility.GetIsInspectorExpanded(src))
                return;

            if (!Trigger.ShowLines)
                return;

            foreach (Trigger trigger in src.Triggers)
            {
                if (trigger != null)
                {
                    UtilsEditor.DrawArrowLine(src.transform.position, trigger.transform.position);
                }
            }

            UtilsEditor.DrawEventLine(src.transform, src.OnInteract);
        }

    }
}