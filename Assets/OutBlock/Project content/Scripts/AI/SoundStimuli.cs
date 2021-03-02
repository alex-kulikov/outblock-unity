using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    [System.Serializable]
    public class SoundProperties
    {

        [SerializeField]
        private bool loud = false;
        public bool Loud { get => loud; set => loud = value; }
        [SerializeField]
        private float distance = 5;
        public float Distance { get => distance; set => distance = value; }
        [SerializeField]
        private float multiplier = 1;
        public float Multiplier { get => multiplier; set => multiplier = value; }

    }

    /// <summary>
    /// Data of the sound stimuli
    /// </summary>
    public class SoundStimuli
    {

        /// <summary>
        /// Source of the sound
        /// </summary>
        public Transform caster { get; private set; }

        /// <summary>
        /// Is it loud(Can be hearable by all entities within the zone)
        /// </summary>
        public bool loud { get; private set; }

        /// <summary>
        /// Max distance of the sound
        /// </summary>
        public float distance { get; private set; }

        /// <summary>
        /// Loudness multiplier. Same sound with same loudness can be prioritised with this parameter.
        /// </summary>
        public float multiplier { get; private set; }

        public SoundStimuli(Transform caster, SoundProperties properties)
        {
            this.caster = caster;
            loud = properties.Loud;
            distance = properties.Distance;
            multiplier = properties.Multiplier;
        }

    }
}