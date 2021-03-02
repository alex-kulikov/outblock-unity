using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class contains sounds and allow to other classes play them by id or name.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundContainer : MonoBehaviour
    {

        /// <summary>
        /// Sound data.
        /// </summary>
        [System.Serializable]
        public struct Sound
        {

            /// <summary>
            /// Name of the sound.
            /// </summary>
            public string id;

            /// <summary>
            /// Priority of the sound. Localized to the sound container.
            /// </summary>
            [Range(0, 128)]
            public int priority;

            /// <summary>
            /// Volume range of the sound.
            /// </summary>
            [Range(0, 1)]
            public float volume;

            /// <summary>
            /// AudioClips of the sound. Plays random clip if more than one.
            /// </summary>
            public AudioClip[] clips;
        }

        [SerializeField]
        private Sound[] sounds = new Sound[0];
        [SerializeField]
        private AudioSource source = null;
        [SerializeField, Range(0, 1)]
        private float volume = 1;

        private int lastPriority;

        /// <summary>
        /// Is sound playing right now?
        /// </summary>
        public bool IsPlaying()
        {
            return source != null && source.isPlaying;
        }

        /// <summary>
        /// Stop any sound
        /// </summary>
        public void StopSound()
        {
            if (!source)
                return;

            source.Stop();
        }

        /// <summary>
        /// Play sound by its name
        /// </summary>
        public void PlaySound(string index)
        {
            if (!source)
                return;

            int ind = -1;
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].id == index)
                    ind = i;
            }

            if (ind < 0)
                return;

            if (source.isPlaying && sounds[ind].priority > lastPriority)
                return;

            if (sounds[ind].clips.Length <= 0)
                return;

            lastPriority = sounds[ind].priority;
            source.Stop();
            source.volume = sounds[ind].volume * volume;
            source.clip = sounds[ind].clips.Length > 1 ? sounds[ind].clips[Random.Range(0, sounds[ind].clips.Length)] : sounds[ind].clips[0];
            source.Play();
        }
    }
}