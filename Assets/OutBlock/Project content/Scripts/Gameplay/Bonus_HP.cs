using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Health pick up.
    /// </summary>
    public sealed class Bonus_HP : Bonus
    {

        ///<inheritdoc/>
        protected override void OnPickUp(GameObject go)
        {
            RagdollPart ragdollPart = go.GetComponent<RagdollPart>();
            if (ragdollPart)
            {
                if (ragdollPart.root.player && ragdollPart.root.player.Heal())
                {
                    base.OnPickUp(go);
                }
            }
            else
            {
                Player player = go.GetComponent<Player>();
                if (player && player.Heal())
                    base.OnPickUp(go);
            }
        }
    }
}