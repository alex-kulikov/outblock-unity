using UnityEngine;
using System.Collections;

namespace OutBlock
{

    /// <summary>
    /// Sleep state of an entity. Zzzzzzzzz...
    /// </summary>
    public class AIStateSleep : AIState
    {

        ///<inheritdoc/>
        public override string DebugStatus => "<color=cyan>Zzz...</color>";

        private float maxTime;
        private float checkNearTimeout;

        private const float checkNearCooldown = 0.5f;

        /// <summary>
        /// Sleep timer multiplier.
        /// </summary>
        public float Multiplier { get; set; } = 1;

        /// <summary>
        /// Range from 0 to 1. Represents time before the entity will wake up. 0 - wake up, 1 - sleeping
        /// </summary>
        public float TimerRatio => entity.Data.sleepTimer / maxTime;

        private void WakeUp()
        {
            controller?.Relax();
            entity.assignedZone.Attention(entity.transform.position);
            End();
        }

        private void CheckNear()
        {
            checkNearTimeout = checkNearCooldown;

            foreach (AIEntity entity in entity.assignedZone.assignedAIs)
            {
                if (entity.Sleeping || entity.RagdollEnabled || entity == this.entity)
                    continue;

                float dist = Vector3.Distance(this.entity.transform.position, entity.transform.position);

                if (dist < 1)
                {
                    WakeUp();
                    break;
                }
            }
        }

        //Wake up and switch to attention on sound
        private void Sensors_OnHearSound(SoundStimuli obj)
        {
            entity.Data.lastSeenPlayerPosition = obj.caster.position;
            controller?.SwitchState(controller?.attentionState);
        }

        ///<inheritdoc/>
        public override void Init(AIEntity entity)
        {
            base.Init(entity);

            entity.Data.sleepTimer = maxTime = entity.Data.SleepTimeRange.GetRandom() * Multiplier;
            //entity.sensors.OnHearSound += Sensors_OnHearSound;
            //Clear movement
            controller?.Stop();
            controller?.AnimationAnchor.Animator.SetTrigger("SleepStart");
            controller?.AnimationAnchor.Animator.SetBool("Sleep", true);
        }

        ///<inheritdoc/>
        public override void Update()
        {
            base.Update();
            //Switch to idle state if sleep is over
            entity.Data.sleepTimer -= Time.deltaTime;
            //Check if someone near
            if (checkNearTimeout > 0)
                checkNearTimeout -= Time.deltaTime;
            else
                CheckNear();
            //Wake up when its time
            if (entity.Data.sleepTimer <= 0f)
            {
                WakeUp();
            }
        }

        ///<inheritdoc/>
        public override void End()
        {
            base.End();
            //entity.sensors.OnHearSound -= Sensors_OnHearSound;
            controller?.AnimationAnchor.Animator.SetBool("Sleep", false);
        }
    }
}