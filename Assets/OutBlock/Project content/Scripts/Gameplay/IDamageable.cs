using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Event when IDamageable dies. RIP.
    /// </summary>
    public delegate void OnDeath(IDamageable damageable);

    /// <summary>
    /// Interface for the objects that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Get IDamageable instance
        /// </summary>
        IDamageable Damageable();

        /// <summary>
        /// Is this AI is sleeping?
        /// </summary>
        bool Sleeping { get; }

        /// <summary>
        /// Is ragdoll enabled?
        /// </summary>
        bool RagdollEnabled { get; }

        /// <summary>
        /// Receive damage.
        /// </summary>
        void Damage(DamageInfo damageInfo);

        /// <summary>
        /// Get position of this IDamageable
        /// </summary>
        Vector3 GetTargetPos();

        /// <summary>
        /// Health points normalized to the range from 0 to 1
        /// </summary>
        float HPRatio { get; }

        /// <summary>
        /// Fire this event when dying.
        /// </summary>
        event OnDeath onDeath;

        /// <summary>
        /// Is this instance is dead?
        /// </summary>
        bool Dead { get; }

        /// <summary>
        /// Kill this instance.
        /// </summary>
        void Kill();

        /// <summary>
        /// Revive this instance.
        /// </summary>
        void Revive();
    }
}