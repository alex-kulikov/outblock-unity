using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class allows you to play the audio source with additional depth of parametrs.
    /// Also can emit AI hearable sounds.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {

        [SerializeField]
        private AudioSource source = null;
        [SerializeField]
        private bool playOnAwake = false;
        [SerializeField]
        private bool randomClip = false;
        [SerializeField]
        private AudioClip[] clips = new AudioClip[0];
        public AudioClip[] Clips { get => clips; set => clips = value; }
        [SerializeField]
        private bool randomVolume = false;
        [SerializeField]
        private Vector2 volumeRange = default;
        public Vector2 VolumeRange { get => volumeRange; set => volumeRange = value; }
        [SerializeField]
        private bool rangePitch = false;
        [SerializeField]
        private Vector2 pitchRange = default;
        [SerializeField, Header("AI sensor")]
        private bool aiHear = false;
        [SerializeField]
        private SoundProperties soundProperties = default;
        public SoundProperties SoundProperties => soundProperties;

        private void Awake()
        {
            if (playOnAwake)
                Play();
        }

        private void Loud()
        {
            AIZone.HearSound(new SoundStimuli(transform, soundProperties));
        }

        /// <summary>
        /// Play sound
        /// </summary>
        public void Play()
        {
            source.Stop();

            if (randomClip)
                source.clip = clips[Random.Range(0, clips.Length)];

            if (randomVolume)
                source.volume = Random.Range(volumeRange.x, volumeRange.y);

            if (rangePitch)
                source.pitch = Random.Range(pitchRange.x, pitchRange.y);

            if (aiHear)
                Loud();

            source.Play();
        }        

    }
}