using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Can activate cheats.
    /// </summary>
    public class CheatTrigger : Trigger
    {

        [SerializeField, Header("Cheat trigger")]
        private bool disableAll = false;
        [SerializeField, Space]
        private bool noclip = false;
        [SerializeField]
        private bool invisible = false;
        [SerializeField]
        private bool god = false;
        [SerializeField]
        private bool noDamage = false;
        [SerializeField]
        private bool noFallDamage = false;
        [SerializeField]
        private bool disableRagdoll = false;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);

            if (disableAll)
            {
                DevTools.Instance().DisableAll();

                return;
            }

            DevTools.Instance().Noclip = noclip;
            DevTools.Instance().Invisible = invisible;
            DevTools.Instance().God = god;
            DevTools.Instance().NoDamage = noDamage;
            DevTools.Instance().NoFallDamage = noFallDamage;
            DevTools.Instance().DisableRagdoll = disableRagdoll;
        }

    }
}