using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OutBlock
{

    /// <summary>
    /// Climbable rope on the level.
    /// </summary>
    [ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour
    {

        /// <summary>
        /// Climb data. This object contains all needed data for the characters to climb the rope.
        /// </summary>
        public struct ClimbData
        {

            /// <summary>
            /// Start position of the climb.
            /// </summary>
            public Vector3 start { get; private set; }
            /// <summary>
            /// End position of the climb.
            /// </summary>
            public Vector3 end { get; private set; }
            /// <summary>
            /// Transition point where character will be moved after he is done climbing.
            /// </summary>
            public Transform transitionPoint { get; private set; }
            /// <summary>
            /// Speed of the climb.
            /// </summary>
            public float speed { get; private set; }
            /// <summary>
            /// Time of the climb.
            /// </summary>
            public float t { get; private set; }
            /// <summary>
            /// Distance of the climb.
            /// </summary>
            public float dist { get; private set; }
            /// <summary>
            /// Is character going down? Used for animations.
            /// </summary>
            public bool down { get; private set; }

            public ClimbData(Vector3 start, Vector3 end, Transform transitionPoint, float speed, float t, float dist, bool down) : this()
            {
                this.start = start;
                this.end = end;
                this.transitionPoint = transitionPoint;
                this.speed = speed;
                this.t = t;
                this.dist = dist;
                this.down = down;
            }

        }

        /// <summary>
        /// Possible climb animations. ClimbDown is just sliding down.
        /// </summary>
        public enum ClimbAnimations { ClimbUp, ClimbDown };

        [SerializeField, Header("Main")]
        private Transform endPoint = null;
        [SerializeField]
        private Collider endPointCollider = null;
        [SerializeField, Space]
        private bool horizontal = false;
        [SerializeField, Tooltip("Push the player after he reaches the rope start position"), Space]
        private Transform transitionPointStart = null;
        [SerializeField, Tooltip("Push the player after he reaches the rope end position")]
        private Transform transitionPointEnd = null;
        [SerializeField, Space]
        private float climbUpSpeed = 1.2f;
        [SerializeField]
        private ClimbAnimations climbUpAnimation = ClimbAnimations.ClimbUp;
        [SerializeField, Space]
        private float climbDownSpeed = 4;
        [SerializeField]
        private ClimbAnimations climbDownAnimation = ClimbAnimations.ClimbDown;

        private OffMeshLink link;
        private LineRenderer lineRenderer;
        private Vector3 oldPos;

        private void OnEnable()
        {
            UpdateVisuals();

            if (endPoint)
                oldPos = transform.position;

            if (Application.isPlaying)
            {
                if (link)
                    link.activated = true;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                if (link)
                    link.activated = false;
            }
        }

        private void OnValidate()
        {
            UpdateVisuals();
        }

        private void Update()
        {
            if (endPoint && oldPos != endPoint.position)
            {
                UpdateVisuals();
                oldPos = endPoint.position;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (transitionPointStart)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transitionPointStart.position);
            }
            if (endPoint && transitionPointEnd)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(endPoint.position, transitionPointEnd.position);
            }
        }

        private void UpdateVisuals()
        {
            if (!endPoint)
                return;

            if (!lineRenderer)
                lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint.position);
        }

        [ContextMenu("Create AI Points")]
        private void CreateAIPoints()
        {
            if (link)
                return;

            link = gameObject.AddComponent<OffMeshLink>();

            GameObject goStart = new GameObject("AIStartPoint");
            goStart.transform.SetParent(transform);
            goStart.transform.localPosition = Vector3.zero;

            GameObject goEnd = new GameObject("AIEndPoint");
            goEnd.transform.SetParent(transform);
            goEnd.transform.localPosition = endPoint ? endPoint.localPosition : Vector3.up;

            link.startTransform = goStart.transform;
            link.endTransform = goEnd.transform;
        }

        [ContextMenu("Sync End Point Collider")]
        private void SyncEndPointCollider()
        {
            if (!endPoint || !endPointCollider)
                return;

            switch (endPointCollider)
            {
                case BoxCollider boxCollider:
                    boxCollider.center = endPoint.localPosition;
                    break;

                case SphereCollider sphereCollider:
                    sphereCollider.center = endPoint.localPosition;
                    break;

                case CapsuleCollider capsuleCollider:
                    capsuleCollider.center = endPoint.localPosition;
                    break;
            }
        }

        /// <summary>
        /// Calculate climb data for the character.
        /// </summary>
        public ClimbData CalculateClimbing(Transform actor)
        {
            if (actor && endPoint)
            {
                Vector3 start = transform.position;
                Vector3 end = endPoint.position - Vector3.up * 2.4f;

                //check if the rope start position is higher than the endpoint
                //if it is then swap the start and end points
                bool reversed = transform.position.y > endPoint.position.y;
                if (reversed)
                {
                    start = endPoint.position;
                    end = transform.position - Vector3.up * 2.4f;
                }

                if (horizontal)
                    start -= Vector3.up * 2.4f;

                //calculate rope distance and actor distance to end of the rope
                float maxDist = Vector3.Distance(start, end);
                float actorDist = Vector3.Distance(actor.position, end);

                //calculate the actor normalized position relative to the ropee
                float t = Mathf.Clamp01(1f - actorDist / maxDist);

                //if the actor distance to the end of the rope is less than half of the rope distance
                //then he is on the end of the rope and he is going down
                ClimbData climbData;
                if (actorDist < maxDist * 0.5f)
                {
                    climbData = new ClimbData(end, start, transitionPointStart, climbDownSpeed, 1 - t, maxDist, climbDownAnimation == ClimbAnimations.ClimbDown);
                }
                else
                {
                    climbData = new ClimbData(start, end, transitionPointEnd, climbUpSpeed, t, maxDist, climbUpAnimation == ClimbAnimations.ClimbDown);
                }
                return climbData;
            }

            return new ClimbData();
        }

        /// <summary>
        /// Execute climbing for the player if he exists.
        /// </summary>
        public void StartClimb()
        {
            Player.Instance?.ClimbRope(CalculateClimbing(Player.Instance.transform));
        }

    }
}