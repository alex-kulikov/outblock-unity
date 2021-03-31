using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Sends damage.
    /// </summary>
    public class DamageTrigger : Trigger
    {

        [SerializeField, Header("Damage trigger")]
        private DamageInfo damage;
        /// <summary>
        /// Damage info.
        /// </summary>
        public DamageInfo Damage
        {
            get => damage;
            set => damage = value;
        }

        private IDamageable damageableBuffer;

        private void Start()
        {
            damage.force = transform.TransformDirection(damage.force);
            damage.team = Teams.Both;
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            damage.source = transform.position;
            IDamageable damageable = other.GetComponent<IDamageable>();
            damageable?.Damage(damage);
        }

    }
}