using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Pick up which fires the UnityEvent when picked up.
    /// </summary>
    public class Bonus_Event : Bonus
    {

        [SerializeField]
        private UnityEvent onPickUp = default;

        ///<inheritdoc/>
        protected override void OnPickUp(GameObject go)
        {
            onPickUp?.Invoke();
            base.OnPickUp(go);
        }

    }
}