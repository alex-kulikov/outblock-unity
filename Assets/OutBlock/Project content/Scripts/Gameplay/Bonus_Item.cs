using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Item pick up.
    /// </summary>
    public class Bonus_Item : Bonus
    {

        [SerializeField, Header("Bonus item")]
        private string item = "";
        [SerializeField]
        private bool showInInventory = true;
        [SerializeField, Tooltip("Item color")]
        private Color color = Color.white;
        [SerializeField, Tooltip("GUI pickup text. {0} will be replaced with the item name.")]
        private string pickUpText = "You picked up the {0}";
        [SerializeField, Tooltip("GUI consume text. {0} will be replaced with the required item count. {1} will be replaced with the item name.")]
        private string consumeText = "{0}x {1} has been consumed";

        private void AddItem(GameObject go)
        {
            Player player = go.GetComponent<Player>();
            if (player)
            {
                player.Inventory.AddItem(new Item(item, color, showInInventory, pickUpText, consumeText));
                base.OnPickUp(go);
            }
        }

        protected override void OnPickUp(GameObject go)
        {
            RagdollPart ragdollPart = go.GetComponent<RagdollPart>();
            if (ragdollPart)
            {
                if (ragdollPart.root.player)
                {
                    AddItem(ragdollPart.root.player.gameObject);
                }
            }
            else
            {
                AddItem(go);
            }
        }

    }

}