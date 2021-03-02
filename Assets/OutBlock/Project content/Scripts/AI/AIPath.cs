using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Patrol path for the AI entities
    /// </summary>
    public class AIPath : MonoBehaviour
    {

        /// <summary>
        /// Possible behaviours for the patrol path
        /// </summary>
        public enum Behaviours { Loop, PingPong, Stop, Switch };

        /// <summary>
        /// Point of the path
        /// </summary>
        [System.Serializable]
        public class PathNode
        {

            [SerializeField]
            private Vector3 pos = Vector3.zero;
            /// <summary>
            /// Position of the point
            /// </summary>
            public Vector3 Pos { get => pos; set => pos = value; }
            [SerializeField, HideInInspector]
            private Vector3 dir;
            /// <summary>
            /// Direction of the point
            /// </summary>
            public Vector3 Dir { get => dir; set => dir = value; }
            [SerializeField]
            private float waitTime = 0;
            /// <summary>
            /// How much an entity should wait before going to the next point
            /// </summary>
            public float WaitTime => waitTime;

            /// <summary>
            /// Create PathNode object
            /// </summary>
            /// <param name="pos">Position of the point</param>
            /// <param name="dir">Direction of the point</param>
            /// <param name="waitTime">How much an entity should wait before going to the next point</param>
            public PathNode(Vector3 pos, Vector3 dir, float waitTime)
            {
                this.pos = pos;
                this.dir = dir;
                this.waitTime = waitTime;
            }

        }

        [SerializeField]
        private List<PathNode> pathNodes = new List<PathNode>();
        public List<PathNode> PathNodes => pathNodes;
        [SerializeField]
        private Behaviours behaviour = Behaviours.Loop;
        [SerializeField]
        private AIPath nextPath = null;

        private int index = -1;
        private int iterator = 1;

        private void OnDrawGizmosSelected()
        {
            if (pathNodes == null || pathNodes.Count <= 0)
                return;

            for (int i = 0; i < pathNodes.Count; i++)
            {
                Vector3 pos = transform.TransformPoint(pathNodes[i].Pos);

                if (i < pathNodes.Count - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(pos, transform.TransformPoint(pathNodes[i + 1].Pos));
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(pos, 0.25f);
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < pathNodes.Count; i++)
            {
                if (pathNodes[i].Dir == Vector3.zero)
                    pathNodes[i].Dir = Vector3.forward;
            }
        }

        /// <summary>
        /// Go to the next point
        /// </summary>
        /// <param name="newPath">Link to the next path if its exists</param>
        public PathNode Next(out AIPath newPath)
        {
            newPath = null;
            if (!gameObject.activeSelf)
            {
                return null;
            }

            index += iterator;

            if (index >= pathNodes.Count || index < 0)
            {
                switch (behaviour)
                {

                    case Behaviours.Loop:
                        index = 0;
                        break;

                    case Behaviours.PingPong:
                        iterator *= -1;
                        index += iterator * 2;
                        break;

                    case Behaviours.Stop:
                        return null;

                    case Behaviours.Switch:
                        index = -1;
                        newPath = nextPath;
                        if (nextPath)
                            return nextPath.Next(out AIPath temp);
                        else return null;

                }
            }

            return new PathNode(transform.TransformPoint(pathNodes[index].Pos), transform.TransformDirection(pathNodes[index].Dir), pathNodes[index].WaitTime);
        }

#if UNITY_EDITOR
        [ContextMenu("Update path")]
        private void UpdatePath()
        {
            List<Transform> sorted = transform.Cast<Transform>().OrderBy(x => x.GetSiblingIndex()).ToList();
            foreach (Transform child in sorted)
            {
                UnityEditor.Undo.RecordObject(this, "Updated path");
                pathNodes.Add(new PathNode(transform.InverseTransformPoint(child.transform.position), transform.InverseTransformDirection(child.transform.forward), 2));
                UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
#endif

    }
}