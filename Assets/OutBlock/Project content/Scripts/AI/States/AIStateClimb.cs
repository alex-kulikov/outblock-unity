using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// State for climbing a rope
    /// </summary>
    public class AIStateClimb : AIState
    {

        /// <summary>
        /// Previous entity state
        /// </summary>
        public AIState prevState { get; set; }

        /// <summary>
        /// Rope climb data
        /// </summary>
        public Rope.ClimbData climbData { get; set; }

        ///<inheritdoc/>
        public override string DebugStatus => "";

        ///<inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);

            if (controller)
            {
                controller.NavMeshAgent.speed = climbData.speed;
                controller.AnimationAnchor.Animator.SetBool(climbData.down ? "RopeClimbDown" : "RopeClimb", true);
                controller.AnimationAnchor.Animator.SetBool(!climbData.down ? "RopeClimbDown" : "RopeClimb", false);
            }
        }

        ///<inheritdoc/>
        public override void Update()
        {
            base.Update();
            if (controller && !controller.NavMeshAgent.isOnOffMeshLink)
            {
                controller.SwitchState(prevState);
            }
        }

        ///<inheritdoc/>
        public override void End()
        {
            base.End();
            if (controller)
            {
                if (climbData.transitionPoint)
                    controller.ForceMoveTo(climbData.transitionPoint.position);
                else controller.Stop();
                controller.AnimationAnchor.Animator.SetBool("RopeClimbDown", false);
                controller.AnimationAnchor.Animator.SetBool("RopeClimb", false);
            }
        }

    }
}