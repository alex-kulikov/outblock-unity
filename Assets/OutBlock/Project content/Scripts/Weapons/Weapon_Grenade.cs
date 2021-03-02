using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Grenades.
    /// </summary>
    public class Weapon_Grenade : Weapon
    {

        [SerializeField, Header("Grenade")]
        private float explosionRadius = 3;
        [SerializeField, Tooltip("Calculate easy trajectory from start to target without physics and bounces.")]
        private bool simpleMode = false;
        [SerializeField, Tooltip("Throw position offset.")]
        private Vector3 offset = new Vector3(0.5f, 0, 0);
        [SerializeField, Tooltip("Throw rotation offset.")]
        private Vector2 angleOffset = new Vector3(-20, 20);
        [SerializeField, Tooltip("Time before grenade explodes.")]
        private float explosionTime = 2;
        [SerializeField, Tooltip("Damage trigger settings that will be applied to the explosion.")]
        private Trigger.TriggerSettings damageTriggerSettings = null;
        [SerializeField, Tooltip("Initial throw speed. Meters per second."), Header("Physics")]
        private float grenadeSpeed = 6;
        [SerializeField, Tooltip("Speed decrease over time. Meters per second.")]
        private float speedDecrease = 1;
        [SerializeField, Range(0, 1), Tooltip("How much velocity grenade will lose after bouncing of the surface?")]
        private float bounceSpeedDecrease = 0.4f;
        [SerializeField, Tooltip("How much gravity affects the grenade.")]
        private float gravityMultiplier = 1;
        [SerializeField, Tooltip("Initial throw speed multiplier relative to the throw angle. X - for small angle(parallel to the ground), Y - for high.")]
        private Vector2 speedAngleRange = new Vector2(0.2f, 1.2f);
        [SerializeField, Header("Display")]
        private LineRenderer trajectoryRenderer = null;
        [SerializeField]
        private GameObject hitPoint = null;
        [SerializeField, Tooltip("Use this transform for angle reference."), Space]
        private Transform referenceTransform = null;

        private Ray ray;
        private List<TrajectoryPoint> trajectory = new List<TrajectoryPoint>();
        private Vector3 target;
        private bool request;
        private System.Action<List<TrajectoryPoint>> callback;

        private void Start()
        {
            if (referenceTransform == null)
                referenceTransform = transform.root;
        }

        private void FixedUpdate()
        {
            if (request)
            {
                request = false;

                trajectory.Clear();

                StartCoroutine(simpleMode ? SimpleTrajectoryCalculating() : TrajectoryCalculating());
            }
        }

        /// <summary>
        /// Calculates more complex, physically-based trajectory. Used by player.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TrajectoryCalculating()
        {
            if (!referenceTransform)
                yield return 0;

            float trajectorySpeed = grenadeSpeed;
            float trajectoryTime = 0;
            Vector3 gravity = Vector3.zero;
            float timeSlice = Time.fixedDeltaTime * 4;

            Vector3 direction = Quaternion.AngleAxis(angleOffset.y, referenceTransform.up) * (Quaternion.AngleAxis(angleOffset.x, referenceTransform.right) * ray.direction);
            direction.Normalize();

            Vector3 originOffset = referenceTransform.right * offset.x + referenceTransform.up * offset.y + referenceTransform.forward * offset.z;
            trajectory.Add(new TrajectoryPoint(ray.origin + originOffset, false, 0));

            float angle = -Vector3.SignedAngle(referenceTransform.forward, direction, referenceTransform.right) + 45;
            trajectorySpeed *= Mathf.Lerp(speedAngleRange.x, speedAngleRange.y, Mathf.Clamp01(angle / 90f));

            while ((trajectorySpeed > 0 || gravity.magnitude > 0) && trajectoryTime < explosionTime)
            {
                Vector3 point = trajectory[trajectory.Count - 1].point;
                Vector3 nextPoint = point + (direction * trajectorySpeed + gravity) * timeSlice;
                direction = (nextPoint - point).normalized;
                float dist = Vector3.Distance(point, nextPoint);

                bool bounce = false;
                if (Physics.SphereCast(point, 0.1f, direction, out RaycastHit hit, dist, layerMask))
                {
                    bounce = true;
                    point = hit.point + hit.normal * 0.1f;
                    direction = Vector3.Reflect(direction, hit.normal).normalized;
                    trajectorySpeed *= 1f - bounceSpeedDecrease;
                    gravity *= 0;
                }
                else
                {
                    point = nextPoint;
                    gravity += Physics.gravity * timeSlice * gravityMultiplier;
                }

                trajectorySpeed -= speedDecrease * timeSlice;
                trajectoryTime += timeSlice;

                trajectory.Add(new TrajectoryPoint(point, bounce, trajectoryTime));
            }

            callback?.Invoke(trajectory);
            yield return null;
        }

        /// <summary>
        /// Calculate simple parabola trajectory. Used by AI.
        /// </summary>
        private IEnumerator SimpleTrajectoryCalculating()
        {
            target += Vector3.right * Random.Range(-1.5f, 1.5f) + Vector3.forward * Random.Range(-1.5f, 1.5f);
            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, 100, layerMask))
            {
                target = hit.point;
            }
            target += Vector3.up * 0.1f;

            float gravity = Mathf.Abs(Physics.gravity.y);

            float distance = Vector3.Distance(transform.position, target);
            float angle = 45;
            float v = Mathf.Sqrt(gravity * distance / Mathf.Sin(2 * angle));
            float v2 = v * v;

            float time = 0;
            float maxTime = 1.41f * v / gravity;

            float maxHeight = v2 * Mathf.Pow(Mathf.Sin(angle), 2) / (2 * gravity);
            float timeSlice = Time.fixedDeltaTime * 4;

            AnimationCurve parabolaCurve = new AnimationCurve(new Keyframe(0, 0, 2, 2), new Keyframe(1, 1, 0, 0));

            trajectory.Add(new TrajectoryPoint(ray.origin, false, 0));

            int c = 100;
            while(time < maxTime || time < explosionTime)
            {
                time += timeSlice;
                float t = time / maxTime;

                Vector3 point = Vector3.Lerp(transform.position, target, t);
                float y = t <= 0.5f ? t / 0.5f : (1 - t) / 0.5f;
                point.y += parabolaCurve.Evaluate(y) * maxHeight;

                trajectory.Add(new TrajectoryPoint(point, false, time));

                if (--c <= 0)
                {
                    Debug.LogError(string.Format("Failed to build a trajectory! V:{0} MH:{1} MT:{2} || T:{3} TS:{4}", v, maxHeight, maxTime, time, timeSlice));
                    break;
                }
            }

            callback?.Invoke(trajectory);
            yield return null;
        }

        private void UpdateTrajectoryRenderer(List<TrajectoryPoint> path)
        {
            trajectoryRenderer.positionCount = path.Count;
            trajectoryRenderer.SetPositions(path.ConvertAll(x => x.point).ToArray());
            hitPoint.transform.position = path[path.Count - 1].point;
            hitPoint.SetActive(path.Count > 1);
        }

        private void Throw(List<TrajectoryPoint> path)
        {
            if (gunEffect)
            {
                Invoke("ThrowEffect", 1);
            }

            nextFire = Time.time + delay;

            if (team == Teams.Player && DevTools.Instance().God)
                return;

            if (--ammo <= 0 && reservedAmmo > 0)
            {
                Reload();
            }
        }

        private void ThrowEffect()
        {
            if (!referenceTransform)
                return;

            if (!referenceTransform.gameObject.activeSelf)
                return;

            IDamageable damageable = referenceTransform.GetComponent<IDamageable>();
            if (damageable != null && damageable.Dead)
                return;

            Transform effect = Instantiate(gunEffect, transform.position, Quaternion.identity);
            effect.GetComponent<GrenadeEffect>().Set(trajectory, explosionTime, explosionRadius, damageInfo, damageTriggerSettings);
        }

        ///<inheritdoc/>
        protected override bool FireWeapon(Ray ray)
        {
            if (!CanFire())
            {
                return false;
            }

            GetTrajectory(Throw, ray);
            return true;
        }

        /// <summary>
        /// Calculate trajectory.
        /// </summary>
        /// <param name="callback">Callbacks the <see cref="System.Action"/> with the calculated trajectory.</param>
        /// <param name="ray">Input ray.</param>
        public void GetTrajectory(System.Action<List<TrajectoryPoint>> callback, Ray ray)
        {
            if (Utils.CompareRays(this.ray, ray))
            {
                callback?.Invoke(trajectory);
                return;
            }

            this.ray = ray;
            this.callback = callback;
            request = true;
        }

        ///<inheritdoc/>
        public override bool Fire(Vector3 target)
        {
            this.target = target;
            return base.Fire(target);
        }

        ///<inheritdoc/>
        public override void Aim(Transform cam, bool aiming)
        {
            base.Aim(cam, aiming);

            if (aiming)
            {
                trajectoryRenderer.gameObject.SetActive(true);
                GetTrajectory(UpdateTrajectoryRenderer, CamToRay(cam));
            }
            else
            {
                trajectoryRenderer.gameObject.SetActive(false);
            }
        }

    }
}