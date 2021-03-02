#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Denver.Metrics
{
    public class LevelMetricsTreeView : TreeView
    {

        private List<LevelMetricsTreeElement> treeElements;

        public LevelMetricsTreeView(TreeViewState state, List<LevelMetricsTreeElement> treeElements) : base(state)
        {
            this.treeElements = treeElements;

            Reload();
        }

        private void CellGUI(Rect rowRect, LevelMetricsTreeElement item, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref rowRect);

            Rect toggleRect = new Rect(rowRect);
            toggleRect.x += GetContentIndent(item) - 14;
            toggleRect.width = 30;
            if (toggleRect.xMax < rowRect.xMax)
                item.visible = EditorGUI.Toggle(toggleRect, item.visible);

            rowRect.x += 20;
            args.rowRect = rowRect;
            GUI.enabled = item.IsVisible();
            base.RowGUI(args);
            GUI.enabled = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupDepthsFromParentsAndChildren(treeElements[0]);

            return treeElements[0];
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            CellGUI(args.rowRect, (LevelMetricsTreeElement)args.item, ref args);
        }

        protected override void DoubleClickedItem(int id)
        {
            if (!treeElements[id].focusable)
                return;

            Bounds frameBounds = new Bounds(treeElements[id].pos, Vector3.one * 2);
            SceneView.lastActiveSceneView.Frame(frameBounds, false);
        }

    }
}
#endif