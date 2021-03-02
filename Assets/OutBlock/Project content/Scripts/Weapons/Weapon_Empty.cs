using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Empty weapon.
    /// </summary>
    public class Weapon_Empty : Weapon
    {

        ///<inheritdoc/>
        protected override void Init()
        {
            base.Init();
            ammo = maxAmmo;
        }

        ///<inheritdoc/>
        protected override bool FireWeapon(Ray ray)
        {
            return false;
        }

        ///<inheritdoc/>
        public override void Reload()
        {
        }

        ///<inheritdoc/>
        public override bool CanFire()
        {
            return true;
        }

        ///<inheritdoc/>
        public override int GetAmmo()
        {
            return 1;
        }

        ///<inheritdoc/>
        public override string GetAmmoUI()
        {
            return "";
        }

        ///<inheritdoc/>
        public override void SetAmmo(int ammo)
        {
            return;
        }

        ///<inheritdoc/>
        public override bool Reloading()
        {
            return false;
        }

    }
}