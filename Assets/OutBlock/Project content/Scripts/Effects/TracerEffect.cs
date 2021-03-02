using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Creates muzzle flash effect with the bullet tracer.
    /// </summary>
    public class TracerEffect : GunEffect
    {

        [SerializeField]
        private Transform impactEffect = null;
        [SerializeField]
        private Transform bodyImpactEffect = null;
        [SerializeField]
        private LineRenderer tracer = null;

        private Vector3 localHitPos;
        private float t = 0;
        private bool done;

        private void Update()
        {
            if (t <= 1)
            {
                Vector3 pos0 = Vector3.Lerp(startPos, hitPos, t);
                Vector3 pos1 = Vector3.Lerp(startPos, hitPos, t - 0.25f);
                tracer.SetPosition(0, pos0);
                tracer.SetPosition(1, pos1);
                t += 0.25f;
            }
            else if (!done)
            {
                HideTracer();

                Transform effect;
                if (hitObject)
                {
                    if (Utils.CompareTags(hitObject, "Player", "Enemy", "Body"))
                    {
                        effect = Instantiate(bodyImpactEffect, hitPos, Quaternion.identity);
                        Destroy(effect.gameObject, 10);
                    }
                    else
                    {
                        effect = Instantiate(impactEffect, hitPos, Quaternion.identity);
                        effect.GetChild(0).eulerAngles += Vector3.forward * Random.Range(0, 360);
                        effect.forward = normal;
                        effect.SetParent(hitObject);
                        effect.localPosition = localHitPos;
                        Destroy(effect.gameObject, 10);
                    }
                }

                done = true;
            }
        }

        private void HideTracer()
        {
            tracer.gameObject.SetActive(false);
        }

        ///<ingeritdoc/>
        public override void Set(Vector3 startPos, Vector3 hitPos, Vector3 normal, Transform hitObject)
        {
            base.Set(startPos, hitPos, normal, hitObject);
            if (hitObject)
                localHitPos = hitObject.InverseTransformPoint(hitPos);
        }
    }
}