#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Denver.Metrics
{
    public class LevelMetricsTreeElement : TreeViewItem
    {

        public bool focusable { get; private set; } = false;
        public Vector3 pos { get; private set; } = Vector3.zero;
        public bool visible { get; set; } = true;

        public void Reset()
        {
            if (children != null)
                children.Clear();
            depth = 0;
        }

        public bool IsVisible()
        {
            LevelMetricsTreeElement parent = (LevelMetricsTreeElement)this.parent;
            if (parent != null)
                return visible && parent.IsVisible();
            else return visible;
        }

        public void SetPos(Vector3 pos)
        {
            this.pos = pos;
            focusable = true;
        }

    }
}
#endif