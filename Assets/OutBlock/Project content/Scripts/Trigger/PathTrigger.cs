using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Trigger an AI to follow the path.
    /// </summary>
    public class PathTrigger : Trigger
    {

        [SerializeField, Header("Path trigger")]
        private AIPath path = null;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);

            if (path == null)
                return;

            AIController controller = other.GetComponent<AIController>();
            controller?.SetPath(path);
        }

    }
}