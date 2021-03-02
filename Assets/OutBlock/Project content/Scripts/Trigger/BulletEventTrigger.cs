using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Activates when receiving the damage.
    /// </summary>
    public class BulletEventTrigger : EventTrigger, IDamageable
    {

        [SerializeField, Header("Bullet event trigger")]
        private DamageInfo.DamageTypes bulletType = DamageInfo.DamageTypes.Common;

        ///<inheritdoc/>
        public bool RagdollEnabled => false;
        ///<inheritdoc/>
        public bool Sleeping => false;
        ///<inheritdoc/>
        public float HPRatio => 1;
        ///<inheritdoc/>
        public event OnDeath onDeath;
        ///<inheritdoc/>
        public bool Dead => false;

        ///<inheritdoc/>
        public void Damage(DamageInfo damageInfo)
        {
            if (bulletType == damageInfo.damageType)
                Activate((Transform)null);
        }

        ///<inheritdoc/>
        public void Kill()
        {
        }

        /// <inheritdoc/>
        public void Revive()
        {
        }

        ///<inheritdoc/>
        public IDamageable Damageable()
        {
            return null;
        }

        ///<inheritdoc/>
        public Vector3 GetTargetPos()
        {
            return transform.position;
        }
    }
}