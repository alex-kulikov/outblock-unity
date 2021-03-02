using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Base class for the AI states.
    /// </summary>
    public abstract class AIState
    {
        
        /// <summary>
        /// Link to the owner as AIEntity
        /// </summary>
        protected AIEntity entity;

        /// <summary>
        /// Link to the owner as AIController(if exists)
        /// </summary>
        protected AIController controller;

        /// <summary>
        /// Tick method. Happens every frame if the state is active
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Initialize the state
        /// </summary>
        public virtual void Init(AIEntity entity)
        {
            //Linking owner of this state
            this.entity = entity;
            if (entity is AIController)
                controller = (AIController)entity;
        }

        /// <summary>
        /// End this state
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Debug status with this string or display it above the AI Entity
        /// </summary>
        public abstract string DebugStatus
        {
            get;
        }
    }
}