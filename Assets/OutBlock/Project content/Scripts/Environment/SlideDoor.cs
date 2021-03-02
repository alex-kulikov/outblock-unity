using UnityEngine;
using System.Collections.Generic;

namespace OutBlock
{

    /// <summary>
    /// Sliding door.
    /// </summary>
    public class SlideDoor : Activateable
    {

        /// <summary>
        /// Animation ease types.
        /// </summary>
        public enum Eases { Linear, Square, Root };

        [SerializeField, Tooltip("Position for door in local space when opened"), Header("Slider Door")]
        protected Vector3 openOffset = default;
        [SerializeField, Tooltip("Animation flow")]
        protected Eases ease = Eases.Linear;
        [SerializeField, Tooltip("Time of animation")]
        private float time = 2;
        [SerializeField, Tooltip("Time before door changes its state")]
        private float switchDelay = 2;
        [SerializeField]
        private bool once = false;
        [SerializeField, Header("Sounds")]
        private AudioClip openSound = null;
        [SerializeField]
        private AudioClip closeSound = null;

        protected Vector3 start;
        protected Vector3 old;
        protected Vector3 target;
        protected float t = 0.5f;
        private List<ISignalSource> signals = new List<ISignalSource>();

        private bool oldOpen;
        private bool inAction;
        private float delay;

        private void Start()
        {
            time = 1f / time;
            Init();
            target = start;
        }

        private void Update()
        {
            if (delay > 0)
                delay -= Time.deltaTime;

            if (Active && !inAction && delay <= 0)
                Open = signals.Count > 0;

            if (oldOpen != Open)
            {
                inAction = true;
                oldOpen = Open;
                UpdateOld();
                target = Open ? Offset() : start;
                t = 0;
                Utils.PlaySound(transform.position, Open ? openSound : closeSound, 0.4f, 1, 1, 15);
            }

            if (t < 1)
                t += Time.deltaTime * time;

            if (!CheckDifference())
            {
                Animation();
            }
            else if (inAction)
            {
                inAction = false;
                delay = switchDelay;
            }
        }

        ///<inheritdoc/>
        protected override void Reload()
        {
            signals.Clear();
        }

        /// <summary>
        /// Initialize start state.
        /// </summary>
        protected virtual void Init()
        {
            start = transform.localPosition;
        }

        /// <summary>
        /// Position for door in local space when opened
        /// </summary>
        protected virtual Vector3 Offset()
        {
            return openOffset;
        }

        /// <summary>
        /// Update start position for the animation.
        /// </summary>
        protected virtual void UpdateOld()
        {
            old = transform.localPosition;
        }

        /// <summary>
        /// Check if animation is done.
        /// </summary>
        protected virtual bool CheckDifference()
        {
            return transform.localPosition == target;
        }

        /// <summary>
        /// Animate door.
        /// </summary>
        protected virtual void Animation()
        {
            switch (ease)
            {
                case Eases.Linear:
                    transform.localPosition = Vector3.Lerp(old, target, t);
                    break;

                case Eases.Root:
                    transform.localPosition = Vector3.Lerp(old, target, Mathf.Sqrt(t));
                    break;

                case Eases.Square:
                    transform.localPosition = Vector3.Lerp(old, target, Mathf.Pow(t, 2));
                    break;
            }
        }

        ///<inheritdoc/>
        public override void Activate(ISignalSource source)
        {
            if (TargetTeam != Teams.Both && TargetTeam != source.Team)
                return;

            if (!signals.Contains(source))
                signals.Add(source);

            if (Active && once)
                done = true;
        }

        ///<inheritdoc/>
        public override void Deactivate(ISignalSource source)
        {
            signals.Remove(source);
        }
    }
}