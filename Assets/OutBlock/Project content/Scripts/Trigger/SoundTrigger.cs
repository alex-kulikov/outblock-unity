using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Plays the sound.
    /// </summary>
    public class SoundTrigger : Trigger
    {

        [SerializeField, Header("Sound trigger")]
        private AudioSource source = null;
        [SerializeField]
        private SoundProperties soundProperties = default;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            source.Play();
            AIZone.HearSound(new SoundStimuli(transform, soundProperties));
        }

    }
}