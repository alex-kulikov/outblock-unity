using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Base class for the AI entities.
    /// </summary>
    public abstract class AIEntity : MonoBehaviour, IDamageable, ISignalSource, ISaveable
    {

        /// <summary>
        /// Represents base save data class for AI entities.
        /// </summary>
        public class EntitySaveData : DamageableSaveData
        {

            /// <summary>
            /// AI state.
            /// </summary>
            public AIState state { get; private set; }

            /// <summary>
            /// Create new EntitySaveData object.
            /// </summary>
            public EntitySaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, float hp, bool dead, AIState state) : base(id, pos, rot, active, enabled, hp, dead)
            {
                this.state = state;
            }
        }

        /// <summary>
        /// This class holds the events of the AI entity. OnDeath, OnSleep, OnAttention, OnAgression, OnCover, OnRagdoll
        /// </summary>
        [System.Serializable]
        public class Events
        {

            [SerializeField]
            private UnityEvent onDeath = default;
            public UnityEvent OnDeath => onDeath;
            [SerializeField]
            private UnityEvent onSleep = default;
            public UnityEvent OnSleep => onSleep;
            [SerializeField]
            private UnityEvent onAttention = default;
            public UnityEvent OnAttention => onAttention;
            [SerializeField]
            private UnityEvent onAgression = default;
            public UnityEvent OnAgression => onAgression;
            [SerializeField]
            private UnityEvent onCover = default;
            public UnityEvent OnCover => onCover;
            [SerializeField]
            private UnityEvent onRagdoll = default;
            public UnityEvent OnRagdoll => onRagdoll;
            [SerializeField]
            private UnityEvent onRopeClimb = default;
            public UnityEvent OnRopeClimb => onRopeClimb;

        }

        /// <summary>
        /// Can this entity attack the player?
        /// </summary>
        public abstract bool CanAttack { get; }

        /// <summary>
        /// The entity will ignore attacking order from the AIManager
        /// </summary>
        public abstract bool IgnoreAttackOrder { get; }

        /// <summary>
        /// Get current AI state
        /// </summary>
        public abstract AIState GetState();

        /// <summary>
        /// Switch state to the aggression
        /// </summary>
        public abstract void Aggression();

        /// <summary>
        /// Switch state to the attention
        /// </summary>
        public abstract void Attention();

        ///<inheritdoc/>
        public abstract IDamageable Damageable();

        ///<inheritdoc/>
        public abstract bool RagdollEnabled { get; }

        ///<inheritdoc/>
        public abstract void Damage(DamageInfo damageInfo);

        ///<inheritdoc/>
        public abstract void Kill();

        ///<inheritdoc/>
        public abstract void Revive();

        ///<inheritdoc/>
        public abstract Vector3 GetTargetPos();

        /// <inheritdoc/>
        public abstract void Register();

        /// <inheritdoc/>
        public abstract void Unregister();

        ///<inheritdoc/>
        public abstract SaveData Save();

        ///<inheritdoc/>
        public abstract void Load(SaveData data);

        ///<inheritdoc/>
        public abstract Transform AssignedTransform { get; }

        ///<inheritdoc/>
        public abstract bool Sleeping { get; }

        ///<inheritdoc/>
        public abstract float HPRatio { get; }

        ///<inheritdoc/>
        public abstract event OnDeath onDeath;

        ///<inheritdoc/>
        public abstract bool Dead { get; protected set; }

        ///<inheritdoc/>
        public abstract int Id { get; set; }

        ///<inheritdoc/>
        public abstract GameObject GO { get; }

        [SerializeField]
        protected AIAgentData data;
        public AIAgentData Data => data;
        [SerializeField]
        protected AISensors sensors;
        public AISensors Sensors => sensors;
        [SerializeField]
        protected bool friendlyFire = false;
        [SerializeField, Space]
        protected Events events = new Events();

        public Teams Team => Teams.AI;

        public bool InFear => data.fearTimer > 0;

        public AIZone assignedZone { get; set; }

        /// <summary>
        /// Checks if the entity can sleep.
        /// </summary>
        public bool CanSleep()
        {
            return !(data.SleepTimeRange.min == 0 && data.SleepTimeRange.max == 0);
        }

    }
}