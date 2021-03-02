using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Triggers loading.
    /// </summary>
    public class LoadTrigger : Trigger
    {

        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            SaveLoad.Instance().Load();
        }

    }
}