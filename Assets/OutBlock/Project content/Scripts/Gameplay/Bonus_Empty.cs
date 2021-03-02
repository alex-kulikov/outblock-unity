using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Empty pick up with no logic.
    /// </summary>
    public sealed class Bonus_Empty : Bonus
    {

        ///<inheritdoc/>
        protected override void OnPickUp(GameObject go)
        {
            base.OnPickUp(go);
        }

    }
}