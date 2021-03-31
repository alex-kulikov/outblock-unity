using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This object stores damage information.
    /// </summary>
    [System.Serializable]
    public struct DamageInfo
    {

        /// <summary>
        /// Damage types.
        /// </summary>
        public enum DamageTypes { Common, Sleep, Force };

        /// <summary>
        /// How much damage to deal.
        /// </summary>
        public float damage;

        /// <summary>
        /// Applied force.
        /// </summary>
        public Vector3 force;

        /// <summary>
        /// Should be force applied from the center of the damage point? Useful for the explosions.
        /// </summary>
        public bool forceFromCenter;

        /// <summary>
        /// Damage type. 
        /// </summary>
        public DamageTypes damageType;

        /// <summary>
        /// Where we hit something?
        /// </summary>
        public Vector3 hitPoint { get; private set; }

        /// <summary>
        /// Source of the damage.
        /// </summary>
        public Vector3 source { get; set; }

        /// <summary>
        /// Team of the source of the damage.
        /// </summary>
        public Teams team { get; set; }

        public DamageInfo(float damage, Vector3 source, Vector3 hitPoint)
        {
            this.damage = damage;
            this.source = source;
            this.hitPoint = hitPoint;
            team = Teams.Both;
            force = Vector3.zero;
            forceFromCenter = false;
            damageType = DamageTypes.Common;
        }

        public DamageInfo(float damage, Vector3 source, Vector3 hitPoint, Teams team) : this(damage, source, hitPoint)
        {
            this.team = team;
        }

        public DamageInfo(float damage, Vector3 source, Vector3 hitPoint, Teams team, DamageTypes damageType) : this(damage, source, hitPoint, team)
        {
            force = Vector3.zero;
            this.damageType = damageType;
        }

        public DamageInfo(float damage, Vector3 source, Vector3 hitPoint, Teams team, Vector3 force, DamageTypes damageType) : this(damage, source, hitPoint, team)
        {
            this.force = force;
            this.damageType = damageType;
        }

        public DamageInfo(float damage, Vector3 source, Vector3 hitPoint, Teams team, Vector3 force, bool forceFromCenter, DamageTypes damageType) : this(damage, source, hitPoint, team)
        {
            this.force = force;
            this.damageType = damageType;
            this.forceFromCenter = forceFromCenter;
        }

        public DamageInfo(DamageInfo damageInfo)
        {
            damage = damageInfo.damage;
            source = damageInfo.source;
            hitPoint = damageInfo.hitPoint;
            force = damageInfo.force;
            forceFromCenter = damageInfo.forceFromCenter;
            damageType = damageInfo.damageType;
            team = damageInfo.team;
        }
    }
}