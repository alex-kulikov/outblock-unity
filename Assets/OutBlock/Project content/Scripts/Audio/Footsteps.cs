using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class plays correct footstep sounds based on the animation(walking, running and e.t.c) and of the surface.
    /// </summary>
    public class Footsteps : MonoBehaviour
    {

        [SerializeField]
        private Animator animator = null;
        [SerializeField]
        private float minVolume = 0.025f;
        [SerializeField]
        private float maxVolume = 0.05f;
        [SerializeField]
        private string forward = "";
        [SerializeField]
        private AudioClip[] defaultSteps = new AudioClip[0];
        [SerializeField]
        private AudioClip[] grassSteps = new AudioClip[0];
        [SerializeField]
        private SoundEmitter footstepSource = null;

        /// <summary>
        /// Play footstep sound
        /// </summary>
        /// <param name="id">0 - walk, 1 - strafe, 2 - run</param>
        public void FootStep(int id)
        {
            if (!footstepSource)
                return;

            float absF = Mathf.Abs(animator.GetFloat(forward));
            int i = -1;
            if (absF >= 0.1f && absF <= 0.5f)
                i = 0;
            else if (absF > 0.5f)
                i = 2;
            else i = 1;

            if (i != id)
                return;

            if (grassSteps.Length > 0)
            {
                if (Physics.CheckSphere(transform.position, 0.2f, LayerMask.GetMask("Grass")))
                {
                    footstepSource.Clips = grassSteps;
                }
                else
                {
                    footstepSource.Clips = defaultSteps;
                }
            }

            float volume = Mathf.Lerp(minVolume, maxVolume, absF / 2);
            footstepSource.VolumeRange = Vector2.one * volume;
            footstepSource.SoundProperties.Distance = absF > 0.4f ? Mathf.Lerp(0, 2f,absF / 2) : 0;
            footstepSource.Play();
        }
    }
}