
namespace OutBlock
{

    /// <summary>
    /// Stores ammo for the weapon type. Used in the ammo bonuses.
    /// </summary>
    [System.Serializable]
    public struct WeaponAmmo
    {
        /// <summary>
        /// For which weapon this ammo is?
        /// </summary>
        public Weapon.WeaponTypes weaponType;

        /// <summary>
        /// How much mags?
        /// </summary>
        public int mags;
    }
}