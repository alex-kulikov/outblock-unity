using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Base class for the AI behaviours
    /// </summary>
    public abstract class AIBehaviour
    {

        /// <summary>
        /// Link to the owner
        /// </summary>
        protected AIEntity entity;

        /// <summary>
        /// Is this behaviour can be performed?
        /// </summary>
        public abstract bool CanDo();

        /// <summary>
        /// Perform behaviour
        /// </summary>
        public abstract void DoAction();

        /// <summary>
        /// Create AIBehaviour object and link to the owner
        /// </summary>
        public AIBehaviour(AIEntity entity)
        {
            this.entity = entity;
        }

    }
}