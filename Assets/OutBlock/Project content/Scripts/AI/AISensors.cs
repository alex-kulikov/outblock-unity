using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Eyes and ears of the entity
    /// </summary>
    public class AISensors : MonoBehaviour
    {

        /// <summary>
        /// Action performed when entity hear sound
        /// </summary>
        public event System.Action<SoundStimuli> OnHearSound;

        /// <summary>
        /// Inside which vision cone target is
        /// </summary>
        public enum SensorType { Inner, Outer };

        /// <summary>
        /// Which part of the player is visible
        /// </summary>
        public enum VisionPoint { Center, Feet, Head };

        /// <summary>
        /// Stores result of the vision calculation
        /// </summary>
        public struct VisionResult
        {
            /// <summary>
            /// Is player visible?
            /// </summary>
            public bool playerVisible { get; set; }

            /// <summary>
            /// Spotted attention point
            /// </summary>
            public Transform attentionPoint { get; set; }

            /// <summary>
            /// Spotted fear point
            /// </summary>
            public Transform fearPoint { get; set; }

            /// <see cref="SensorType"/>
            public SensorType sensorType { get; set; }

            /// <see cref="VisionPoint"/>
            public VisionPoint visionPoint { get; set; }
        }

        [SerializeField]
        private AIEntity entity = null;
        [SerializeField, Tooltip("Outer cone of vision"), Header("Vision")]
        private float visionInnerConeAngle = 35f;
        [SerializeField, Tooltip("Inner cone of vision")]
        private float visionOuterConeAngle = 70f;
        [SerializeField, Tooltip("Additional vision angle when player is visible")]
        private float visionAdditionalConeAngle = 15;
        [SerializeField]
        private float visionVerticalAngleMultiplier = 1;
        [SerializeField, Tooltip("Vision distance")]
        private float visionDistance = 10f;
        public float VisionDistance => visionDistance;
        [SerializeField, Header("Vision rect")]
        private Vector3 visionRectSize = new Vector3(2, 2, 3);
        [SerializeField]
        private float visionRectAggressionMultiplier = 2;
        [SerializeField]
        private LayerMask visionRectLayerMask = default;
        [SerializeField, Space, Tooltip("Eyes position")]
        private Transform visionPivot = null;
        public Transform VisionPivot => visionPivot;
        [SerializeField, Tooltip("Vision pivot rotation smoothing. The higher the value the faster the rotation")]
        private float visionPivotSmoothing = 4;
        [SerializeField, Tooltip("Mask for vision obstacles")]
        private LayerMask visionLayerMask = default;
        public LayerMask VisionLayerMask => visionLayerMask;

        [SerializeField, Tooltip("Hearing cylinder radius"), Header("Hearing")]
        private float hearingRadius = 3f;
        [SerializeField, Tooltip("Hearing cylinder positional offset")]
        private Vector3 hearingOffset = Vector3.zero;
        [SerializeField, Tooltip("Hearing cylinder height")]
        private float hearingHeight = 3;
        [SerializeField]
        private float hearingAttentionMultiplier = 2;

        /// <summary>
        /// Spot distance of the player, when hiding in a bush, in the aggression state
        /// </summary>
        public const float hidingDistAggression = 6;

        /// <summary>
        /// Spot distance of the player, when hiding in a bush, in the attention state
        /// </summary>
        public const float hidingDistAttention = 4;

        /// <summary>
        /// Angle of the vision inner cone with additional angles
        /// </summary>
        public float VisionInnerConeAngle => visionInnerConeAngle + (visionResult.playerVisible ? visionAdditionalConeAngle : 0);

        /// <summary>
        /// Angle of the vision outer cone with additional angles
        /// </summary>
        public float VisionOuterConeAngle => visionOuterConeAngle + (visionResult.playerVisible ? visionAdditionalConeAngle : 0);

        /// <summary>
        /// Hearing distance of the entity after all multipliers
        /// </summary>
        public float HearingRadius => hearingRadius * (entity.GetState() is AIStateIdle || entity.GetState() == null ? 1 : hearingAttentionMultiplier);

        /// <summary>
        /// Position of the hearing cylinder.
        /// </summary>
        public Vector3 HearingPosition => transform.position + hearingOffset;

        /// <summary>
        /// Top position of the hearing cylinder.
        /// </summary>
        public Vector3 HearingTopPosition => HearingPosition + Vector3.up * hearingHeight;

        /// <summary>
        /// Result of the vision calculation
        /// </summary>
        public VisionResult visionResult { get; private set; }

        private Vector3 VisionRectSize => visionRectSize * (entity.GetState() is AIStateAttention ? 1 : visionRectAggressionMultiplier);
        private Bounds VisionRectBounds => new Bounds(transform.position + transform.forward * VisionRectSize.z / 2, VisionRectSize);

        private RaycastHit[] visionRaycastResults = new RaycastHit[25];
        private Vector3 visionRot;
        private float lastNoiseLevel;
        private float noiseLevelTimeout;

        private void Start()
        {
            visionRot = transform.eulerAngles;
        }

        private void Update()
        {
            visionRot = Utils.AngleLerp(visionRot, transform.eulerAngles, Time.deltaTime * visionPivotSmoothing);
            visionPivot.eulerAngles = visionRot;

            if (noiseLevelTimeout > 0)
            {
                noiseLevelTimeout -= Time.deltaTime;
            }
            else
            {
                lastNoiseLevel = 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Utils.GizmosDrawCircle(HearingPosition, HearingRadius);
            Utils.GizmosDrawCircle(HearingTopPosition, HearingRadius);

            Gizmos.matrix = Matrix4x4.TRS(visionPivot.position, visionPivot.rotation, Vector3.one);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.forward * visionRectSize.z / 2, visionRectSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.forward * VisionRectSize.z / 2, VisionRectSize);

            Gizmos.matrix = Matrix4x4.TRS(visionPivot.position, visionPivot.rotation, new Vector3(1, visionVerticalAngleMultiplier, 1));
            Gizmos.color = Color.cyan;
            Gizmos.DrawFrustum(Vector3.zero, visionOuterConeAngle, 0, visionDistance, 1);
            Gizmos.color = Color.red;
            Gizmos.DrawFrustum(Vector3.zero, visionInnerConeAngle, 0, visionDistance, 1);
        }

        private bool CheckPoint(Vector3 point, out float dist, out SensorType sensorType)
        {
            dist = Vector3.Distance(point, visionPivot.position);
            sensorType = SensorType.Outer;

            if (dist > visionDistance)
                return false;

            float hAngle, vAngle;
            hAngle = vAngle = 0;

            Vector3 dir = (point - visionPivot.position).normalized;
            //Calculating angles
            //Horizontal
            Vector3 pointH = point;
            pointH.y = visionPivot.position.y;
            hAngle = Vector3.Angle((pointH - visionPivot.position).normalized, visionPivot.forward);
            //Vertical
            float sideA = Vector3.Distance(visionPivot.position, pointH);
            float sideB = point.y - visionPivot.position.y;
            vAngle = Mathf.Abs(Mathf.Atan(sideB / sideA) * Mathf.Rad2Deg);

            bool inRect = false;
            if (!(entity.GetState() is AIStateIdle))
            {
                inRect = VisionRectBounds.Contains(point);
            }

            Ray ray = new Ray(visionPivot.position, dir);
            bool visible = !Physics.Raycast(ray, dist, visionLayerMask);

            if (visible)
            {
                if (inRect)
                {
                    sensorType = SensorType.Inner;
                    return true;
                }

                if (dist > visionDistance)
                    return false;

                if (hAngle > VisionOuterConeAngle / 2f || vAngle > VisionOuterConeAngle / 2f * visionVerticalAngleMultiplier)
                    return false;

                if (hAngle <= VisionInnerConeAngle / 2f && vAngle <= VisionInnerConeAngle / 2f * visionVerticalAngleMultiplier)
                {
                    sensorType = SensorType.Inner;
                }

                return true;
            }

            return false;
        }

        private GameObject[] GetAttentionPoints()
        {
            return GameObject.FindGameObjectsWithTag("AttentionPoint");
        }

        private GameObject[] GetFearPoints()
        {
            return GameObject.FindGameObjectsWithTag("FearPoint");
        }

        /// <summary>
        /// Reset vision result and vision pivot angle
        /// </summary>
        public void ResetVision()
        {
            visionPivot.eulerAngles = transform.eulerAngles;
            visionRot = transform.eulerAngles;
            visionResult = new VisionResult();
        }

        /// <summary>
        /// Check vision
        /// </summary>
        public VisionResult CheckVision()
        {
            VisionResult result = new VisionResult();
            float dist = 0;
            SensorType sensorType = SensorType.Inner;

            if (DevTools.Instance().Invisible || Player.Instance == null)
            {
                result.playerVisible = false;
            }
            else
            {
                bool visible = false;
                for (int i = 0; i <= 3; i++)
                {
                    Vector3 pos;
                    switch (i)
                    {
                        case 1:
                            pos = Player.Instance.GetFeetPos();
                            break;

                        case 2:
                            pos = Player.Instance.GetHeadPos();
                            break;

                        default:
                            pos = Player.Instance.GetTargetPos();
                            break;
                    }

                    visible = CheckPoint(pos, out dist, out sensorType);
                    result.visionPoint = (VisionPoint)i;
                    if (visible)
                        break;
                }

                if (visible && Player.Instance.hiding)
                {
                    float hidingDist = (entity.GetState() is AIStateAggresion || entity.GetState() is AIStateCover ? hidingDistAggression : hidingDistAttention);
                    if (dist > hidingDist)
                        visible = false;
                }

                result.playerVisible = visible;
                result.sensorType = sensorType;
            }

            GameObject[] points = GetAttentionPoints();
            if (points.Length > 0)
            {
                foreach(GameObject go in points)
                {
                    if (!go.activeSelf)
                        continue;

                    if (go.transform.IsChildOf(transform))
                        continue;

                    if (CheckPoint(go.transform.position, out dist, out sensorType))
                    {
                        if (sensorType == SensorType.Outer)
                        {
                            result.attentionPoint = go.transform;
                            break;
                        }
                    }
                }
            }

            points = GetFearPoints();
            if (points.Length > 0)
            {
                foreach (GameObject go in points)
                {
                    if (!go.activeSelf)
                        continue;

                    if (go.transform.IsChildOf(transform))
                        continue;

                    if (CheckPoint(go.transform.position, out dist, out sensorType))
                    {
                        if (sensorType == SensorType.Outer)
                        {
                            result.fearPoint = go.transform;
                            break;
                        }
                    }
                }
            }

            visionResult = result;
            return visionResult;
        }

        /// <summary>
        /// Check if sound is hearable
        /// </summary>
        public void HearSound(SoundStimuli stimuli)
        {
            if (stimuli.loud)
            {
                OnHearSound?.Invoke(stimuli);
                return;
            }

            if (stimuli.distance <= 0)
                return;

            if (stimuli.caster.position.y < HearingPosition.y || stimuli.caster.position.y > HearingTopPosition.y)
                return;

            float maxDistance = HearingRadius + stimuli.distance;
            float distance = Vector3.Distance(stimuli.caster.position, transform.position);

            if (distance > maxDistance)
                return;

            float k = (1 - (distance / maxDistance)) * stimuli.multiplier;
            if (k >= 0.5f && k > lastNoiseLevel)
            {
                lastNoiseLevel = k;
                noiseLevelTimeout = 5;
                OnHearSound?.Invoke(stimuli);
            }
        }
    }
}