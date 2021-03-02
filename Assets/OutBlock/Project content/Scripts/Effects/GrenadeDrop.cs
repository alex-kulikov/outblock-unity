using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Spawns grenade which is going to go down.
    /// </summary>
    public sealed class GrenadeDrop : Instantiator
    {

        [SerializeField, Header("Grenade drop")]
        private DamageInfo damageInfo = default;
        [SerializeField, Space, Tooltip("Settings for the damage trigger.")]
        private Trigger.TriggerSettings triggerSettings = null;
        [SerializeField, Space, Tooltip("Time before grenade explode.")]
        private float explosionTime = 2;
        [SerializeField]
        private float explosionRadius = 3;
        [SerializeField, Tooltip("Spawn grenade right above the player.")]
        private bool dropAbovePlayer = true;
        [SerializeField]
        private float randomRadius = 1f;

        /// <summary>
        /// Spawning grenade and calculate falling trajectory for it. Pos and rot parameters will be ignored.
        /// </summary>
        protected override Transform Create(Transform prefab, Vector3 pos, Quaternion rot)
        {
            Transform clone = base.Create(prefab, pos, rot);

            //DropTrajectory
            List<TrajectoryPoint> trajectory = new List<TrajectoryPoint>();

            Vector3 startPos = dropAbovePlayer && Player.Instance ? Player.Instance.transform.position : transform.position;
            startPos += new Vector3(Random.Range(-randomRadius, randomRadius), 0, Random.Range(-randomRadius, randomRadius));

            trajectory.Add(new TrajectoryPoint(startPos, false, 0));

            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Default")))
            {
                float t = Mathf.Sqrt(2 * hit.distance / -Physics.gravity.y);
                trajectory.Add(new TrajectoryPoint(hit.point, true, t));
            }

            GrenadeEffect grenadeEffect = clone.GetComponent<GrenadeEffect>();
            grenadeEffect.Set(trajectory, explosionTime, explosionRadius, damageInfo, triggerSettings);

            return clone;
        }

    }
}