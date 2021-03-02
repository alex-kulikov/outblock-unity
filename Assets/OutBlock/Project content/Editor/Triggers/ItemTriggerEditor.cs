using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;

namespace OutBlock
{
    [CustomEditor(typeof(ItemTrigger))]
    public class ItemTriggerEditor : Editor
    {

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable | GizmoType.NotInSelectionHierarchy)]
        private static void DrawGizmo(ItemTrigger src, GizmoType gizmoType)
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

            UtilsEditor.DrawEventLine(src.transform, src.OnSuccess);
            UtilsEditor.DrawEventLine(src.transform, src.OnFail);
        }

    }
}