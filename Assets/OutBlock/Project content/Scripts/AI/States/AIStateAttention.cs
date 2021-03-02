using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Attention state. Searching for player or other suspicious activity.
    /// </summary>
    public class AIStateAttention : AIState
    {

        /// <inheritdoc/>
        public override string DebugStatus => "<color=yellow>?</color>";

        /// <summary>
        /// Range from 0 to 1. Indicates normalized time before the entity will calm down. 0 - calm, 1 - attention.
        /// </summary>
        public float Cooldown => entity.Data.currentAttentionWaitTime / entity.Data.AttentionWaitTime;

        private AIWalkRandPos walkRand;

        //Move to sound position if player not in vision
        private void Sensors_OnHearSound(SoundStimuli obj)
        {
            AISensors.VisionResult visionResult = entity.Sensors.CheckVision();
            if (!visionResult.playerVisible)
            {
                entity.Data.currentAttentionWaitTime = entity.Data.AttentionWaitTime;
                entity.Data.lastSeenPlayerPosition = obj.caster.position;
            }
        }

        /// <inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);

            walkRand = new AIWalkRandPos(entity, new Vector2(2, 3), 2.5f);

            //Switch close agents to attention as well
            List<AIEntity> aiInZone = entity.assignedZone.assignedAIs;
            for (int i = 0; i < aiInZone.Count; i++)
            {
                if (aiInZone[i] == entity)
                    continue;

                float distance = Vector3.Distance(entity.transform.position, aiInZone[i].transform.position);
                if (distance < entity.Sensors.HearingRadius)
                {
                    aiInZone[i].Attention();
                }
            }

            //Setup timer to idle
            entity.Data.currentAttentionWaitTime = entity.Data.AttentionWaitTime;
            entity.Data.currentAttentionPlayerSpotWaitTime = entity.Data.AttentionPlayerSpotWaitTime;
            entity.Sensors.OnHearSound += Sensors_OnHearSound;
        }

        /// <inheritdoc/>
        public override void Update()
        {
            //CheckVision
            AISensors.VisionResult visionResult = entity.Sensors.CheckVision();

            if (visionResult.fearPoint != null)
            {
                AIUtils.InitiateFear(entity, visionResult.fearPoint);
            }

            if (entity.InFear)
                entity.Data.fearTimer -= Time.deltaTime;

            if (!entity.InFear)
            {
                if (visionResult.playerVisible)
                {
                    if (Vector3.Distance(entity.transform.position, Player.Instance.transform.position) <= 0.9f)
                        controller?.SpotPlayer();

                    //Move to last seen position
                    controller?.MoveTo(entity.Data.lastSeenPlayerPosition);

                    //Spot with outer vision and reset timers
                    if (visionResult.sensorType == AISensors.SensorType.Outer)
                    {
                        entity.Data.currentAttentionPlayerSpotWaitTime = entity.Data.AttentionPlayerSpotWaitTime;
                        entity.Data.currentAttentionWaitTime = entity.Data.AttentionWaitTime;
                    }
                    //Spot with inner vision and switch to aggression if timer is up
                    else if (visionResult.sensorType == AISensors.SensorType.Inner)
                    {
                        entity.Data.currentAttentionPlayerSpotWaitTime -= Time.deltaTime;
                        if (entity.Data.currentAttentionPlayerSpotWaitTime <= 0)
                        {
                            controller?.SpotPlayer();
                        }
                    }
                    entity.Data.lastSeenPlayerPosition = Player.Instance.transform.position;
                }
                //Relax if player not seen
                else
                {
                    //Move to last seen position with rand position
                    controller?.MoveTo(entity.Data.lastSeenPlayerPosition + walkRand.randPos);
                    walkRand.DoAction();

                    entity.Data.currentAttentionPlayerSpotWaitTime = entity.Data.AttentionPlayerSpotWaitTime;
                    entity.Data.currentAttentionWaitTime -= Time.deltaTime;
                    if (entity.Data.currentAttentionWaitTime <= 0)
                    {
                        controller?.Relax();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void End()
        {
            base.End();
            entity.Sensors.OnHearSound -= Sensors_OnHearSound;
        }
    }
}