using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Base save data.
    /// </summary>
    public class SaveData
    {

        /// <summary>
        /// Saveable ID.
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// Saveable position.
        /// </summary>
        public Vector3 pos { get; private set; }

        /// <summary>
        /// Saveable rotation.
        /// </summary>
        public Vector3 rot { get; private set; }

        /// <summary>
        /// Saveable gameobject state.
        /// </summary>
        public bool active { get; private set; }

        /// <summary>
        /// Saveable component state.
        /// </summary>
        public bool enabled { get; private set; }

        public SaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled)
        {
            this.id = id;
            this.pos = pos;
            this.rot = rot;
            this.active = active;
            this.enabled = enabled;
        }
    }

    /// <summary>
    /// Damageables save data.
    /// </summary>
    public class DamageableSaveData : SaveData
    {

        public float hp { get; private set; }

        public bool dead { get; private set; }

        public DamageableSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, float hp, bool dead) : base(id, pos, rot, active, enabled)
        {
            this.hp = hp;
            this.dead = dead;
        }

    }
}