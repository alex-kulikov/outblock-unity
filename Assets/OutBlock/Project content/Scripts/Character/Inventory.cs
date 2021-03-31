using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Struct to contain weapon.
    /// </summary>
    [System.Serializable]
    public struct WeaponSlot
    {

        [SerializeField]
        private Weapon weapon;
        /// <summary>
        /// Weapon instance.
        /// </summary>
        public Weapon Weapon => weapon;

        [SerializeField]
        private int initMags;
        /// <summary>
        /// How many mags will be given to the player from the start?
        /// </summary>
        public int InitMags => initMags;

        [SerializeField]
        private int maxMags;
        /// <summary>
        /// How many mags can player have for this weapon.
        /// </summary>
        public int MaxMags => maxMags;

    }

    public struct Item
    {

        public string Name { get; set; }
        public Color Color { get; private set; }
        public bool ShowInInventory { get; private set; }
        public string PickUpMessage { get; private set; }
        public string ConsumeMessage { get; private set; }

        public string GetFullName()
        {
            return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(Color), Name);
        }

        public Item(string name, Color color, bool showInInventory, string pickUpMessage, string consumeMessage)
        {
            Name = name;
            Color = color;
            ShowInInventory = showInInventory;
            PickUpMessage = pickUpMessage;
            ConsumeMessage = consumeMessage;
        }
    }

    /// <summary>
    /// Inventory class for the player. It contains WeaponSlots and items as strings.
    /// </summary>
    public class Inventory : MonoBehaviour
    {

        [SerializeField]
        private WeaponSlot[] weaponSlots = new WeaponSlot[0];
        /// <summary>
        /// Weapon slots.
        /// </summary>
        public WeaponSlot[] WeaponSlots => weaponSlots;

        private TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        private List<Item> items = new List<Item>();

        private void UpdateInventoryDisplay()
        {
            if (items.Count <= 0)
            {
                GameUI.Instance().InventoryItems.text = "";
                return;
            }

            Dictionary<Item, int> itemsDictionary = new Dictionary<Item, int>();

            foreach(Item item in items)
            {
                if (!item.ShowInInventory)
                    continue;

                if (itemsDictionary.ContainsKey(item))
                    itemsDictionary[item]++;
                else itemsDictionary.Add(item, 1);
            }

            string output = "";
            foreach(KeyValuePair<Item, int> pair in itemsDictionary)
            {
                output += string.Format("{0}x {1}\n", pair.Value, pair.Key.GetFullName());
            }

            GameUI.Instance().InventoryItems.text = output;
        }

        /// <summary>
        /// Initiate inventory.
        /// </summary>
        /// <param name="team">Set this team to the weapons.</param>
        public void Init(Teams team)
        {
            foreach(WeaponSlot weaponSlot in weaponSlots)
            {
                weaponSlot.Weapon.team = team;
                weaponSlot.Weapon.AddMags(weaponSlot.InitMags);
            }

            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Update status of the weapon(Selected or not?)
        /// </summary>
        public void UpdateWeaponIndex(int weaponIndex)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                weaponSlots[i].Weapon.gameObject.SetActive(i == weaponIndex);
            }
        }

        /// <summary>
        /// Get the weapon by index.
        /// </summary>
        public Weapon GetWeapon(int weaponIndex)
        {
            return weaponSlots[weaponIndex].Weapon;
        }

        /// <summary>
        /// Get ammo of the weapons.
        /// </summary>
        public int[] GetAmmo()
        {
            int[] result = new int[weaponSlots.Length];
            for (int i = 0; i < weaponSlots.Length; i++)
                result[i] = weaponSlots[i].Weapon.GetAmmo();
            return result;
        }

        /// <summary>
        /// Get reserved ammo of the weapons.
        /// </summary>
        public int[] GetReservedAmmo()
        {
            int[] result = new int[weaponSlots.Length];
            for (int i = 0; i < weaponSlots.Length; i++)
                result[i] = weaponSlots[i].Weapon.reservedAmmo;
            return result;
        }

        /// <summary>
        /// Set ammo and reserved ammo of the weapons
        /// </summary>
        public void SetAmmo(int[] ammos, int[] reservedAmmos)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                weaponSlots[i].Weapon.SetAmmo(ammos[i]);
                weaponSlots[i].Weapon.reservedAmmo = reservedAmmos[i];
            }
        }

        /// <summary>
        /// Add ammo to the weapon.
        /// </summary>
        public void AddAmmo(WeaponAmmo[] ammo)
        {
            foreach (WeaponSlot weaponSlot in weaponSlots)
            {
                for (int i = 0; i < ammo.Length; i++)
                {
                    if (ammo[i].weaponType == weaponSlot.Weapon.WeaponType)
                    {
                        weaponSlot.Weapon.AddMags(ammo[i].mags);
                        if (weaponSlot.Weapon.GetMags() > weaponSlot.MaxMags)
                        {
                            weaponSlot.Weapon.SetMags(weaponSlot.MaxMags);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get list of items in the inventory.
        /// </summary>
        public List<Item> GetItems()
        {
            return new List<Item>(items);
        }

        /// <summary>
        /// Set items of the inventory.
        /// </summary>
        public void SetItems(List<Item> items)
        {
            this.items = new List<Item>(items);
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Check if the item in the inventory. Case insensitive.
        /// </summary>
        public bool HasItem(string item, int count = 1)
        {
            return IndexOfItem(item).Count >= count;
        }

        /// <summary>
        /// Index of the item in the inventory. Returns -1 if this item is not present in the inventory.
        /// </summary>
        public List<Item> IndexOfItem(string item)
        {
            return items.Where(x => x.Name.Equals(item, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Add item to the inventory.
        /// </summary>
        public void AddItem(Item item)
        {
            items.Add(item);
            UpdateInventoryDisplay();
            GameUI.Instance().ShowMessage(string.Format(item.PickUpMessage, item.GetFullName()));
        }

        /// <summary>
        /// Use item from the inventory
        /// </summary>
        /// <returns>Item was succesfully used.</returns>
        public bool ConsumeItem(string item, int count = 1)
        {
            List<Item> items = IndexOfItem(item);
            if (items.Count >= count)
            {
                GameUI.Instance().ShowMessage(string.Format(items[0].ConsumeMessage, count, items[0].GetFullName()));
                for (int i = 0; i < count; i++)
                {
                    this.items.Remove(items[i]);
                }
                UpdateInventoryDisplay();
                return true;
            }

            return false;
        }

    }
}