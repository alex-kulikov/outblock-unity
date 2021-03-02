using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Ammo pick up.
    /// </summary>
    public sealed class Bonus_Ammo : Bonus
    {

        [SerializeField]
        private WeaponAmmo[] ammo = new WeaponAmmo[0];

        private void Done(GameObject go)
        {
            go.GetComponent<Player>()?.AddAmmo(ammo);
            base.OnPickUp(go);
        }

        ///<inheritdoc/>
        protected override void OnPickUp(GameObject go)
        {
            RagdollPart ragdollPart = go.GetComponent<RagdollPart>();
            if (ragdollPart)
            {
                if (ragdollPart.root.player)
                {
                    Done(ragdollPart.root.player.gameObject);
                }
            }
            else
            {
                Done(go);
            }
        }        

    }
}