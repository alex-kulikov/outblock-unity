using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    
    /// <summary>
    /// Idle entity state. Staying still or patroling a path
    /// </summary>
    public class AIStateIdle : AIState
    {

        //Current point on the parh
        private AIPath.PathNode targetPathNode;
        //Current path
        private AIPath patrolPath;
        private Vector3 lastDir;

        ///<inheritdoc/>
        public override string DebugStatus => "";

        public AIStateIdle(Vector3 initDir)
        {
            lastDir = initDir;
        }

        //Switch to attention state if sound heared
        private void Sensors_OnHearSound(SoundStimuli obj)
        {
            entity.Data.lastSeenPlayerPosition = obj.caster.position;
            controller?.SwitchState(controller?.attentionState);
        }

        ///<inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);
            //Setup events and variables
            entity.Data.waitForSleep = entity.Data.WaitForSleepRange.GetRandom();
            entity.Data.currentPlayerSpotWaitTime = entity.Data.PlayerSpotWaitTime;
            entity.Sensors.OnHearSound += Sensors_OnHearSound;
        }

        ///<inheritdoc/>
        public override void Update()
        {
            //Check if player visible, and switch to attention state if so
            AISensors.VisionResult visionResult = entity.Sensors.CheckVision();
            if (visionResult.playerVisible)
            {
                if (entity.Data.currentPlayerSpotWaitTime > 0)
                {
                    entity.Data.currentPlayerSpotWaitTime -= Time.deltaTime;
                }
                else
                {
                    entity.Data.lastSeenPlayerPosition = Player.Instance.transform.position;
                    controller?.SwitchState(controller?.attentionState);
                    return;
                }
            }
            else
            {
                if (entity.Data.currentPlayerSpotWaitTime < entity.Data.PlayerSpotWaitTime)
                    entity.Data.currentPlayerSpotWaitTime += Time.deltaTime;
            }

            if (visionResult.attentionPoint != null)
            {
                entity.Attention();
                entity.assignedZone.PropogatePlayerPosition(visionResult.attentionPoint.position);
                visionResult.attentionPoint.gameObject.SetActive(false);
                return;
            }

            if (visionResult.fearPoint != null)
            {
                AIUtils.InitiateFear(entity, visionResult.fearPoint);
            }

            if (!entity.InFear)
            {
                //Patrol routine
                if (entity.Data.Path != null)
                {
                    //Select target point
                    patrolPath = entity.Data.Path;
                    if (targetPathNode == null)
                    {
                        targetPathNode = entity.Data.Path.Next(out AIPath newPath);
                        if (newPath != null)
                            entity.Data.Path = newPath;
                        if (targetPathNode != null)
                            entity.Data.pointWaitTime = targetPathNode.WaitTime;
                    }
                    else
                    {
                        //Movement to target point, stay and rotate towards point direction if wait time is > 0
                        controller?.MoveTo(targetPathNode.Pos);
                        if (Vector3.Distance(entity.transform.position, targetPathNode.Pos) < 0.75f)
                        {
                            entity.Data.pointWaitTime -= Time.deltaTime;
                            if (entity.Data.pointWaitTime > 0f)
                            {
                                controller?.RotateTo(targetPathNode.Dir);
                            }
                            else
                            {
                                targetPathNode = null;
                            }
                        }
                    }
                }
                else
                {
                    //Move to spawn point if no path
                    controller?.MoveTo(entity.Data.spawnPoint);
                    if (Vector3.Distance(entity.transform.position, entity.Data.spawnPoint) < 0.75f)
                    {
                        controller?.RotateTo(lastDir);
                    }
                }
            }

            //Sleep routine
            entity.Data.waitForSleep -= Time.deltaTime;
            if (entity.Data.waitForSleep <= 0f)
            {
                controller?.Sleep();
            }
        }

        /// <summary>
        /// Resets the internal patrol logic. Recommended to use when the data.path is changed
        /// </summary>
        public void ResetPath()
        {
            targetPathNode = null;
            patrolPath = null;
        }

        ///<inheritdoc/>
        public override void End()
        {
            base.End();
            lastDir = entity.transform.forward;
            //Clear
            entity.Sensors.OnHearSound -= Sensors_OnHearSound;
        }
    }
}