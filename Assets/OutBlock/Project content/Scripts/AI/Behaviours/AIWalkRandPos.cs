using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This behaviour makes the entity to walk randomly around the point. This behaviour starts timer thats update
    /// random point. With randPos property you can retreive this point. All parameters passed in the constructor.
    /// </summary>
    public class AIWalkRandPos : AIBehaviour
    {

        private float maxDistance;
        private Vector2 updateRange;
        private float randPosTimer;

        /// <summary>
        /// Get random point
        /// </summary>
        public Vector3 randPos { get; private set; }

        /// <summary>
        /// Update random point and reset the timer
        /// </summary>
        private void UpdateRandPos()
        {
            Vector3 newRandPos = Random.insideUnitSphere * maxDistance;
            newRandPos.y = 0;
            randPos = newRandPos;
            randPosTimer = Random.Range(updateRange.x, updateRange.y);
        }

        ///<inheritdoc/>
        public AIWalkRandPos(AIEntity entity, Vector2 updateRange, float maxDistance) : base(entity)
        {
            this.maxDistance = maxDistance;
            this.updateRange = updateRange;
            UpdateRandPos();
        }

        ///<inheritdoc/>
        public override bool CanDo()
        {
            return true;
        }

        ///<inheritdoc/>
        public override void DoAction()
        {
            randPosTimer -= Time.deltaTime;
            if (randPosTimer <= 0)
                UpdateRandPos();
        }
    }
}