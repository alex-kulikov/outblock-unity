using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace OutBlock
{
    public static class UtilsEditor
    {
        
        public static void DrawEventLine(Transform source, UnityEvent unityEvent)
        {
            int count = unityEvent.GetPersistentEventCount();

            for (int i = 0; i < count; i++)
            {
                Object target = unityEvent.GetPersistentTarget(i);
                GameObject go = null;

                if (target is GameObject _go)
                {
                    go = _go;
                }
                else if (target is MonoBehaviour mono)
                {
                    go = mono.gameObject;
                }

                if (go != null)
                {
                    DrawArrowLine(source.position, go.transform.position);
                }
            }
        }

        public static void DrawArrowLine(Vector3 start, Vector3 end)
        {
            Handles.color = Color.white;
            Handles.DrawDottedLine(start, end, 5);

            Vector3 dir = (end - start).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                int arrows = Mathf.RoundToInt(Vector3.Distance(start, end) / 6f);
                for (int j = 0; j < arrows; j++)
                {
                    Handles.ArrowHandleCap(j, start + dir * 6 * j, rot, 1.5f, EventType.Repaint);
                }
            }
        }

    }
}