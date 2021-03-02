using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Cover for the AIs.
    /// </summary>
    public class Cover : MonoBehaviour
    {

        [SerializeField]
        private Mesh viewMesh = null;
        [SerializeField]
        private Vector2 timeInCover = new Vector2(2, 4);
        public Vector2 TimeInCover => timeInCover;

        private bool taken;

        /// <summary>
        /// List of the covers.
        /// </summary>
        public static List<Cover> covers { get; private set; } = new List<Cover>();

        private void OnEnable()
        {
            covers.Add(this);
        }

        private void OnDisable()
        {
            covers.Remove(this);
        }

        private void OnDrawGizmos()
        {
            if (!taken)
                Gizmos.color = Color.yellow * 0.75f;
            else Gizmos.color = Color.red * 0.75f;
            Gizmos.DrawMesh(viewMesh, transform.position, transform.rotation);
        }

        private void Take()
        {
            taken = true;
        }

        /// <summary>
        /// Free this cover.
        /// </summary>
        public void Free()
        {
            taken = false;
        }

        /// <summary>
        /// Find closest cover.
        /// </summary>
        /// <param name="pos">Entity position.</param>
        /// <param name="target">Entity target/enemy.</param>
        /// <param name="maxDist">Max distance for the cover.</param>
        /// <param name="cover">Found cover. Can be null.</param>
        public static bool GetCover(Vector3 pos, Vector3 target, float maxDist, out Cover cover)
        {
            if (covers.Count <= 0)
            {
                cover = null;
                return false;
            }

            List<int> availableCovers = new List<int>();
            for (int i = 0; i < covers.Count; i++)
            {
                if (!covers[i].taken)
                    availableCovers.Add(i);
            }

            if (availableCovers.Count <= 0)
            {
                cover = null;
                return false;
            }

            pos.y = 1;

            Vector3 targetDir = pos - target;
            float minDist = 10000;
            int index = availableCovers[0];
            for (int i = 0; i < availableCovers.Count; i++)
            {
                Vector3 dir = target - covers[availableCovers[i]].transform.position;
                if (Vector3.Dot(targetDir, dir) > 0)
                    continue;

                float dist = Vector3.Distance(covers[availableCovers[i]].transform.position, pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    index = availableCovers[i];
                }
            }

            if (minDist <= maxDist)
            {
                covers[index].Take();
                cover = covers[index];
                return true;
            }
            else
            {
                cover = null;
                return false;
            }
        }

    }
}