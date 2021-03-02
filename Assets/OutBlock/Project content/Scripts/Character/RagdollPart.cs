using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Body part of the ragdoll.
    /// </summary>
    public class RagdollPart : MonoBehaviour, IDamageable
    {

        /// <summary>
        /// Collider of the body part.
        /// </summary>
        public Collider coll { get; private set; }

        /// <summary>
        /// Ragdoll of the body part.
        /// </summary>
        new public Rigidbody rigidbody { get; private set; }

        /// <summary>
        /// Ragdoll root of the ragdoll.
        /// </summary>
        public RagdollRoot root { get; set; }

        public bool RagdollEnabled => false;
        public bool Sleeping => false;
        public float HPRatio => 0;
        public event OnDeath onDeath;
        public bool Dead => false;

        private void Awake()
        {
            coll = GetComponent<Collider>();
            rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Makes this body part kinematic.
        /// </summary>
        public void SetKinematic(bool kinematic)
        {
            rigidbody.isKinematic = kinematic;
            coll.enabled = !kinematic;
        }

        /// <summary>
        /// Send damage to the ragdoll root.
        /// </summary>
        public void Damage(DamageInfo damageInfo)
        {
            root.Damage(damageInfo);
        }

        /// <inheritdoc/>
        public void Kill()
        {
            root.Kill();
        }

        /// <inheritdoc/>
        public void Revive()
        {
        }

        /// <inheritdoc/>
        public IDamageable Damageable()
        {
            return this;
        }

        /// <inheritdoc/>
        public Vector3 GetTargetPos()
        {
            return transform.position;
        }

    }
}