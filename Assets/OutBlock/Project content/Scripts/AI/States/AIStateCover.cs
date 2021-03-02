using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// State when entity in a cover
    /// </summary>
    public class AIStateCover : AIState
    {

        /// <summary>
        /// Current cover
        /// </summary>
        public Cover currentCover { get; set; }

        private float time;
        private bool inCover;
        private bool done;
        private AISensors.VisionResult visionResult;
        private AIWeaponBehaviour weaponBehaviour;

        ///<inheritdoc/>
        public override string DebugStatus => "<color=red>!</color>";

        ///<inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);

            weaponBehaviour = new AIWeaponBehaviour(entity);
            visionResult = new AISensors.VisionResult();
            //set random cover time from cover and move to it
            done = false;
            time = Random.Range(currentCover.TimeInCover.x, currentCover.TimeInCover.y);
            controller?.ForceMoveTo(currentCover.transform.position);
        }

        ///<inheritdoc/>
        public override void Update()
        {
            base.Update();

            //move to cover
            if (!inCover)
            {
                //check if AI in cover yet
                if (Vector3.Distance(entity.transform.position, currentCover.transform.position) <= 1)
                    inCover = true;
            }
            else
            {
                //decreasing the timer
                time -= Time.deltaTime;
                //if we see player and timer is not done then shooting
                visionResult = entity.Sensors.CheckVision();
                controller?.RotateTo((entity.Data.lastSeenPlayerPosition - entity.transform.position).normalized);
                if (time > 0)
                {
                    //entity.data.currentAggresionWaitTime = entity.data.aggresionWaitTime;
                    if (Player.Instance && visionResult.playerVisible)
                    {
                        entity.Data.lastSeenPlayerPosition = Player.Instance.transform.position;
                        entity.assignedZone.PropogatePlayerPosition(entity.Data.lastSeenPlayerPosition);

                        weaponBehaviour.DoAction();
                    }
                }
                else
                {
                    End();
                }
            }
        }

        ///<inheritdoc/>
        public override void End()
        {
            if (done)
                return;

            done = true;
            base.End();
            //release the cover and move to the player
            inCover = false;
            currentCover.Free();
            controller?.MoveTo(entity.Data.lastSeenPlayerPosition);
            if (visionResult.playerVisible)
                controller?.SwitchState(controller?.aggresionState);
            else controller?.SwitchState(controller?.attentionState);
        }
    }
}