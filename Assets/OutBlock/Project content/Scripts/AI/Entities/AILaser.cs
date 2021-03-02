using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Laser entity. It is basically just trap thats sends signal to the all AIs in the zone if the laser touch the player.
    /// </summary>
    public class AILaser : MonoBehaviour, IDamageable
    {

        [SerializeField]
        private float maxDist = 5;
        [SerializeField]
        private LayerMask layerMask = default;
        [SerializeField]
        private LineRenderer laserRay = null;

        private AIZone zone;

        ///<inheritdoc/>
        public bool RagdollEnabled => false;
        ///<inheritdoc/>
        public bool Sleeping => false;
        ///<inheritdoc/>
        public float HPRatio => 1;
        ///<inheritdoc/>
        public event OnDeath onDeath;
        ///<inheritdoc/>
        public bool Dead { get; private set; }

        private void Start()
        {
            maxDist *= transform.localScale.z;
        }

        private void OnEnable()
        {
            zone = AIZone.GetZoneByPos(transform.position);
        }

        private void FixedUpdate()
        {
            if (Dead)
                return;

            if (Physics.Raycast(laserRay.transform.position, laserRay.transform.forward, out RaycastHit hit, maxDist, layerMask))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    zone.ForceAggresion();
                    zone.PropogatePlayerPosition(hit.transform.position);
                }

                laserRay.SetPosition(0, laserRay.transform.position);
                laserRay.SetPosition(1, hit.point);
            }
            else
            {
                laserRay.SetPosition(0, laserRay.transform.position);
                laserRay.SetPosition(1, laserRay.transform.position + laserRay.transform.forward * maxDist);
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(laserRay.transform.position, laserRay.transform.forward * maxDist * transform.localScale.z);
        }

        ///<inheritdoc/>
        public void Kill()
        {
            Dead = true;
            laserRay.enabled = false;
            onDeath?.Invoke(this);
        }

        /// <inheritdoc/>
        public void Revive()
        {
            Dead = false;
            laserRay.enabled = true;
        }

        ///<inheritdoc/>
        public IDamageable Damageable()
        {
            return null;
        }

        ///<inheritdoc/>
        public void Damage(DamageInfo damageInfo)
        {
            if (damageInfo.damageType == DamageInfo.DamageTypes.Common)
                Kill();
        }

        ///<inheritdoc/>
        public Vector3 GetTargetPos()
        {
            return transform.position;
        }
    }
}