using UnityEngine;
using System.Collections;

namespace OutBlock
{

    /// <summary>
    /// Aggression state. Attacking, running, getting in the cover.
    /// </summary>
    public class AIStateAggresion : AIState
    {

        /// <inheritdoc/>
        public override string DebugStatus => "<color=red>!</color>";

        /// <summary>
        /// Range from 0 to 1. Indicates normalized time before the entity will calm down. 0 - calm, 1 - aggression.
        /// </summary>
        public float Cooldown => entity.Data.currentAggresionWaitTime / entity.Data.AggresionWaitTime;

        private AIWeaponBehaviour weaponBehaviour;
        private AIWalkRandPos walkRand;

        private void Sensors_OnHearSound(SoundStimuli obj)
        {
            entity.Data.lastSeenPlayerPosition = obj.caster.position;
        }

        /// <inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);

            weaponBehaviour = new AIWeaponBehaviour(entity);
            walkRand = new AIWalkRandPos(entity, new Vector2(1, 2), 3.5f);

            //Move to last player position and setup timer
            if (!entity.InFear)
                controller?.MoveTo(entity.Data.lastSeenPlayerPosition);
            entity.Data.currentAggresionWaitTime = entity.Data.AggresionWaitTime;

            entity.Sensors.OnHearSound += Sensors_OnHearSound;

            if (controller && Utils.CalculateChance(controller.CoverOnAggressionChance) && !entity.InFear)
                controller.GetCover();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            base.Update();

            AISensors.VisionResult visionResult = entity.Sensors.CheckVision();
            if (visionResult.fearPoint != null)
            {
                AIUtils.InitiateFear(entity, visionResult.fearPoint);
            }

            if (entity.InFear)
                entity.Data.fearTimer -= Time.deltaTime;

            //Attack player if visible
            if (visionResult.playerVisible && Player.Instance != null)
            {
                entity.Data.currentAggresionWaitTime = entity.Data.AggresionWaitTime;
                try
                {
                    entity.Data.lastSeenPlayerPosition = Player.Instance.transform.position;
                }
                catch
                {
                }
                entity.assignedZone.PropogatePlayerPosition(entity.Data.lastSeenPlayerPosition);

                if (!entity.InFear)
                    controller?.MoveTo(!weaponBehaviour.IsDistanceOptimal() ? entity.Data.lastSeenPlayerPosition : entity.transform.position, false);

                weaponBehaviour.DoAction();
            }
            //else move to last seen position
            else
            {
                if (!entity.InFear)
                    controller?.MoveTo(entity.Data.lastSeenPlayerPosition + walkRand.randPos);

                walkRand.DoAction();

                entity.Data.currentAggresionWaitTime -= Time.deltaTime;
                if (entity.Data.currentAggresionWaitTime <= 0)
                {
                    controller?.Relax();
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