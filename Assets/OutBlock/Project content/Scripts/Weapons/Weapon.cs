using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Base class for the weapons.
    /// </summary>
    public class Weapon : MonoBehaviour
    {

        /// <summary>
        /// Possible weapons.
        /// </summary>
        public enum WeaponTypes { Empty, Pistol, Rifle, GrenadeHE, GrenadeSleep, GrenadeBush, GrenadeDistraction };

        [SerializeField, Tooltip("Weapon name")]
        new private string name = "";
        public string Name => name;
        [SerializeField]
        private WeaponTypes weaponType = WeaponTypes.Empty;
        public WeaponTypes WeaponType => weaponType;
        [SerializeField, Tooltip("Weapon damage data")]
        protected DamageInfo damageInfo = default;
        [SerializeField, Tooltip("Physical force of the bullet")]
        private float hitForce = 0;
        [SerializeField, Tooltip("Raycast layerMask")]
        protected LayerMask layerMask = default;
        [SerializeField, Tooltip("Bullet spread")]
        private float spread = 1;
        [SerializeField, Tooltip("Rate of fire. Shots per minute")]
        private float rof = 60;
        [SerializeField, Tooltip("Max bullet dist")]
        private float maxDist = 100;
        [SerializeField, Tooltip("Max ammo in the mag")]
        protected int maxAmmo = 1;
        [SerializeField, Tooltip("Available mags")]
        private int initMags = 10;
        [SerializeField]
        private float reloadTime = 2;
        [SerializeField, Tooltip("Raycast and muzzle fire origin")]
        private Transform muzzle = null;
        [SerializeField, Tooltip("Muzzle fire effect. Not necessary")]
        private Transform muzzleEffect = null;
        [SerializeField, Tooltip("Bullet effect, like tracer and etc")]
        protected Transform gunEffect = null;
        [SerializeField, Tooltip("Player speed multiplier"), Range(0, 1)]
        private float speed = 1;
        public float Speed => speed;
        [SerializeField, Tooltip("Reload sound source")]
        private AudioSource reloadSound = null;

        /// <summary>
        /// Current ammo of the weapon.
        /// </summary>
        protected int ammo;

        /// <summary>
        /// Which team this weapon aligned to?
        /// </summary>
        public Teams team { get; set; }

        /// <summary>
        /// Reserved ammo of the weapon.
        /// </summary>
        public int reservedAmmo { get; set; }

        /// <summary>
        /// Is it aiming right now?
        /// </summary>
        public bool aiming { get; protected set; }

        /// <summary>
        /// Time when the weapon can fire.
        /// </summary>
        protected float nextFire;
        /// <summary>
        /// Delay between the shots.
        /// </summary>
        protected float delay;

        private bool reloading;

        public delegate void OnReload();
        /// <summary>
        /// Called when reloading.
        /// </summary>
        public OnReload onReload;

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            if (ammo == 0 && !reloading)
                Reload();
        }

        private void Update()
        {
            if (reloading && Time.time > nextFire)
                reloading = false;
        }

        /// <summary>
        /// Initialize the weapon.
        /// </summary>
        protected virtual void Init()
        {
            if (initMags > 1)
            {
                ammo = maxAmmo;
                initMags--;
                reservedAmmo = maxAmmo * initMags;
            }
            delay = 60 / rof;
        }

        /// <summary>
        /// Fire the weapon.
        /// </summary>
        protected virtual bool FireWeapon(Ray ray)
        {
            //Do not fire if the weapon not ready
            if (!CanFire())
            {
                return false;
            }

            ray.direction = Quaternion.Euler(muzzle.right * Random.Range(-spread, spread) + muzzle.up * Random.Range(-spread, spread)) * ray.direction;

            Transform effect;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDist, layerMask))
            {
                hit.collider.attachedRigidbody?.AddForceAtPosition(muzzle.forward * hitForce, hit.point);

                IDamageable coll = hit.transform.GetComponent<IDamageable>();
                if (coll != null)
                {
                    coll.Damage(new DamageInfo(damageInfo.damage, muzzle.position, hit.point, team, damageInfo.damageType));
                }
            }

            if (gunEffect)
            {
                effect = Instantiate(gunEffect, muzzle.position, muzzle.rotation);
                if (hit.transform)
                    effect.GetComponent<GunEffect>().Set(muzzle.position, hit.point, hit.normal, hit.transform);
                else
                    effect.GetComponent<GunEffect>().Set(muzzle.position, muzzle.position + ray.direction * maxDist, hit.normal, null);
            }

            if (muzzleEffect)
            {
                Transform mzlEff = Instantiate(muzzleEffect, muzzle.position, muzzle.rotation);
                Destroy(mzlEff.gameObject, 10);
            }

            nextFire = Time.time + delay;

            if (team == Teams.Player && DevTools.Instance().God)
                return true;

            if (--ammo <= 0 && reservedAmmo > 0)
            {
                Reload();
            }

            return true;
        }

        /// <summary>
        /// Creates a <see cref="Ray"/> from camera.
        /// </summary>
        protected Ray CamToRay(Transform cam)
        {
            Ray ray = new Ray();
            Vector3 dir = Quaternion.Euler(cam.right * Random.Range(-spread, spread) + cam.up * Random.Range(-spread, spread)) * cam.forward;
            float muzzleDist = Vector3.Distance(cam.position, muzzle.position);
            ray.origin = cam.position + cam.forward * muzzleDist;
            ray.direction = dir;
            return ray;
        }

        /// <summary>
        /// Start reloading.
        /// </summary>
        public virtual void Reload()
        {
            if (ammo >= maxAmmo || reservedAmmo <= 0)
                return;

            nextFire = Time.time + reloadTime;
            reloading = true;
            reloadSound.Play();
            reservedAmmo += ammo;
            int intermediateAmmo = reservedAmmo > maxAmmo ? maxAmmo : reservedAmmo;
            reservedAmmo -= intermediateAmmo;
            ammo = intermediateAmmo;
            onReload?.Invoke();
        }

        /// <summary>
        /// Add reserved ammo, measured in mags. ReservedAmmo += mags * maxAmmo.
        /// </summary>
        public void AddMags(int mags)
        {
            if (reservedAmmo == 0 && ammo == 0)
            {
                ammo += mags * maxAmmo;
            }
            else
            {
                reservedAmmo += mags * maxAmmo;
            }
        }

        /// <summary>
        /// Set reserved ammo, measured in mags. ReservedAmmo = mags * maxAmmo.
        /// </summary>
        public void SetMags(int mags)
        {
            reservedAmmo = mags * maxAmmo;
        }

        /// <summary>
        /// Is this weapon can fire?
        /// </summary>
        public virtual bool CanFire()
        {
            if (team == Teams.Player && DevTools.Instance().God)
                return Time.time >= nextFire;

            return Time.time >= nextFire && ammo > 0;
        }

        /// <summary>
        /// Is weapon realoding now?
        /// </summary>
        public virtual bool Reloading()
        {
            return reloading;
        }

        /// <summary>
        /// Get current ammo of the weapon.
        /// </summary>
        /// <returns></returns>
        public virtual int GetAmmo()
        {
            return ammo;
        }

        /// <summary>
        /// Get current mags of the weapon.
        /// </summary>
        /// <returns>reservedAmmo / maxAmmo</returns>
        public virtual int GetMags()
        {
            return reservedAmmo / maxAmmo;
        }

        /// <summary>
        /// Converts the ammo and reserved ammo to the readable string.
        /// </summary>
        public virtual string GetAmmoUI()
        {
            return (reloading ? 0 : GetAmmo()) + "/" + reservedAmmo;
        }

        /// <summary>
        /// Set current ammo.
        /// </summary>
        public virtual void SetAmmo(int ammo)
        {
            this.ammo = ammo;
            if (this.ammo > maxAmmo)
                this.ammo = maxAmmo;
        }

        /// <summary>
        /// Set aim state of the weapon.
        /// </summary>
        public virtual void Aim(Transform cam, bool aiming)
        {
            this.aiming = aiming;
        }

        /// <summary>
        /// Fire weapon. Used by player.
        /// </summary>
        public virtual bool Fire(Transform cam)
        {
            Ray ray = CamToRay(cam);

            return FireWeapon(ray);
        }

        /// <summary>
        /// Fire weapon. Used by AI.
        /// </summary>
        public virtual bool Fire(Vector3 target)
        {
            Ray ray = new Ray();
            ray.origin = muzzle.position;
            ray.direction = (target - ray.origin).normalized;

            return FireWeapon(ray);
        }

    }
}