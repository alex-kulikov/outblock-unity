using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Basic hinge door.
    /// </summary>
    public class HingeDoor : SlideDoor
    {

        [SerializeField, Header("Hinge door")]
        private bool sourceRelative = false;

        private Vector3 srcPos;

        ///<inheritdoc/>
        protected override Vector3 Offset()
        {
            if (!sourceRelative)
            {
                return openOffset;
            }
            else
            {
                Vector3 relPos = transform.InverseTransformPoint(srcPos);
                if (relPos.z > 0)
                {
                    return Utils.CorrectAngle(-openOffset);
                }
                else
                {
                    return openOffset;
                }
            }
        }

        ///<inheritdoc/>
        protected override void Init()
        {
            start = transform.localEulerAngles;
        }

        ///<inheritdoc/>
        protected override void UpdateOld()
        {
            old = transform.localEulerAngles;
        }

        ///<inheritdoc/>
        protected override bool CheckDifference()
        {
            return (transform.localEulerAngles - target).magnitude <= 0.01f;
        }

        ///<inheritdoc/>
        protected override void Animation()
        {
            switch (ease)
            {
                case Eases.Linear:
                    transform.localEulerAngles = Utils.AngleLerp(old, target, t);
                    break;

                case Eases.Root:
                    transform.localEulerAngles = Utils.AngleLerp(old, target, Mathf.Sqrt(t));
                    break;

                case Eases.Square:
                    transform.localEulerAngles = Utils.AngleLerp(old, target, Mathf.Pow(t, 2));
                    break;
            }
        }

        ///<inheritdoc/>
        public override void Activate(ISignalSource source)
        {
            base.Activate(source);
            srcPos = source.AssignedTransform.position;
        }

    }
}