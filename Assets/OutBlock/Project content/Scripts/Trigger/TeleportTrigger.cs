using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Teleports the object.
    /// </summary>
    public class TeleportTrigger : Trigger
    {

        [SerializeField, Header("Teleport trigger")]
        private Transform obj = null;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            if (obj)
            {
                RagdollPart ragdollPart = other.GetComponent<RagdollPart>();

                Transform target = ragdollPart ? other.root : other;
                IActor actor = target.GetComponent<IActor>();
                if (actor != null)
                    actor.Warp(obj.position);
                else target.position = obj.position;
            }
        }

    }
}