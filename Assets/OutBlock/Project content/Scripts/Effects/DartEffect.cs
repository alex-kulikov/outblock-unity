using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Creates effect flying dart.
    /// </summary>
    public class DartEffect : GunEffect
    {

        [SerializeField]
        private Transform dart = null;
        [SerializeField]
        private TrailRenderer dartTrail = null;

        private float time;
        private bool done;

        private void Update()
        {
            if (done)
                return;

            time += 0.2f;
            dart.position = Vector3.Lerp(startPos, hitPos, time);
            if (time >= 1)
            {
                done = true;
                if (hitObject)
                {
                    dart.SetParent(hitObject);
                    dartTrail.emitting = false;
                    Destroy(dart.gameObject, 10);
                }
                else Destroy(dart.gameObject);
            }
        }

        private void StartDart()
        {
            dart.position = startPos;
            dart.LookAt(hitPos);
        }

        ///<ingeritdoc/>
        public override void Set(Vector3 startPos, Vector3 hitPos, Vector3 normal, Transform hitObject)
        {
            base.Set(startPos, hitPos, normal, hitObject);
            StartDart();
        }

    }
}