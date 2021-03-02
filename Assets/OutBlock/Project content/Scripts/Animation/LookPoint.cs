using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Look point for the player.
    /// </summary>
    public class LookPoint : MonoBehaviour
    {

        [SerializeField, Range(0, 10)]
        private int priority = 0;
        [SerializeField]
        private Vector3 offset = Vector3.zero;

        public Vector3 position => transform.position + offset;

        private float dist;

        private const float maxDist = 7;

        /// <summary>
        /// List of all look points on the scene.
        /// </summary>
        public static List<LookPoint> LookPoints { get; private set; } = new List<LookPoint>();

        private void OnEnable()
        {
            LookPoints.Add(this);
        }

        private void OnDisable()
        {
            LookPoints.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(position, 0.1f);
        }

        /// <summary>
        /// Get neareast look point by the position.
        /// </summary>
        public static LookPoint GetLookPoint(Vector3 pos)
        {
            if (LookPoints.Count <= 0)
                return null;

            foreach (LookPoint lookPoint in LookPoints)
                lookPoint.dist = Vector3.Distance(pos, lookPoint.position);

            List<LookPoint> points = LookPoints.Where(x => x.dist <= maxDist).OrderBy(s => s.dist - s.priority).ToList();

            if (points.Count <= 0)
                return null;

            return points[0];
        }

    }
}