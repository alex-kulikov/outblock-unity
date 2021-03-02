using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Imitates flying grenade.
    /// </summary>
    public class GrenadeEffect : GunEffect
    {

        [SerializeField]
        private Transform explosionPrefab = null;
        [SerializeField, Header("Trigger collider"), Tooltip("Scale explosionPrefab to the explosion radius from the grenade.")]
        private bool scaleToRadius = false;
        [SerializeField, Tooltip("Destroy explosionPrefab with damage trigger destruction.")]
        private bool destroyEffectWithTrigger = false;

        private Trigger.TriggerSettings triggerSettings;
        private float[] pointsWeight;
        private DamageInfo damageInfo;
        private float explosionRadius;
        private List<Vector3> path;
        private float time;
        private float explosionTime;
        private float flightTime;

        private void Start()
        {
            Invoke("SetFearPoint", 0.5f);
        }

        private void Update()
        {
            if (path != null)
            {
                if (time > 0)
                {
                    transform.position = Utils.GetPathPoint(path, pointsWeight, Mathf.Sqrt(1f - time), out Vector3 dir);
                    time -= Time.deltaTime / flightTime;
                }

                explosionTime -= Time.deltaTime;
                if (explosionTime <= 0)
                {
                    Transform clone = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                    if (scaleToRadius)
                        clone.localScale = Vector3.one * explosionRadius;

                    GameObject damage = new GameObject("Damage");
                    damage.transform.SetParent(clone);
                    damage.transform.localPosition = Vector3.zero;
                    damage.transform.localScale = Vector3.one;
                    damage.layer = 11;

                    SphereCollider coll = damage.AddComponent<SphereCollider>();
                    coll.radius = scaleToRadius ? 1 : explosionRadius;
                    coll.isTrigger = true;

                    DamageTrigger damageTrigger = damage.AddComponent<DamageTrigger>();
                    damageTrigger.Set(triggerSettings);
                    damageTrigger.Damage = damageInfo;

                    Destroyer destroyer = damage.AddComponent<Destroyer>();
                    destroyer.DestroyTime(triggerSettings.DestroyTime);
                    if (destroyEffectWithTrigger)
                        destroyer.onDestroy += () => Destroy(clone.gameObject);

                    Destroy(gameObject);
                }
            }
        }

        private void SetFearPoint()
        {
            transform.tag = "FearPoint";
        }

        ///<inheritdoc/>
        public void Set(List<TrajectoryPoint> path, float explosionTime, float explosionRadius, DamageInfo damageInfo, Trigger.TriggerSettings triggerSettings)
        {
            this.path = path.ConvertAll(x => x.point).ToList();
            flightTime = path[path.Count - 1].time;
            this.explosionTime = explosionTime;
            this.damageInfo = damageInfo;
            this.explosionRadius = explosionRadius;
            this.triggerSettings = triggerSettings;
            time = 1;

            pointsWeight = Utils.GetPointsWeight(this.path, Utils.GetPathDistance(this.path));
        }

    }
}