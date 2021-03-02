using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OutBlock
{

    /// <summary>
    /// This behaviour represent AI's fighting behaviours, deciding which weapon to use and if it is possible.
    /// </summary>
    public class AIWeaponBehaviour : AIBehaviour
    {

        /// <summary>
        /// Distance to the target
        /// </summary>
        private float dist;

        private void ThrowGrenade(AIController controller)
        {
            if (!controller.CanFire())
                return;

            if (!controller.CanThrow())
                return;

            if (!controller.Grenade)
                return;

            Vector3 pos = default;
            switch (controller.Sensors.visionResult.visionPoint)
            {
                case AISensors.VisionPoint.Feet:
                    pos = Player.Instance.GetFeetPos();
                    break;

                case AISensors.VisionPoint.Head:
                    pos = Player.Instance.GetHeadPos();
                    break;

                default:
                    pos = Player.Instance.GetTargetPos();
                    break;
            }

            NavMeshPath path = new NavMeshPath();
            bool success = controller.NavMeshAgent.CalculatePath(pos, path);
            if (success || path.status == NavMeshPathStatus.PathComplete)
                return;

            if (controller.Grenade.Fire(pos))
            {
                controller.Throw();
                controller.StartCoroutine(ThrowAnimation(controller));
            }
        }

        private IEnumerator ThrowAnimation(AIController controller)
        {
            controller.AnimationAnchor.Animator.SetTrigger("Fire");
            controller.AnimationAnchor.Animator.SetInteger("Weapon", (int)controller.Grenade.WeaponType);
            yield return 0;
            controller.AnimationAnchor.Animator.SetInteger("Weapon", (int)controller.Weapon.WeaponType);
        }

        private void FireWeapon(AIController controller)
        {
            if (!controller.CanFire())
                return;

            Vector3 pos = default;
            switch (controller.Sensors.visionResult.visionPoint)
            {
                case AISensors.VisionPoint.Feet:
                    pos = Player.Instance.GetFeetPos();
                    break;

                case AISensors.VisionPoint.Head:
                    pos = Player.Instance.GetHeadPos();
                    break;

                default:
                    pos = Player.Instance.GetTargetPos();
                    break;
            }

            controller.Weapon.Fire(pos);
        }

        public bool IsDistanceOptimal()
        {
            return dist <= entity.Data.ShootingDistanceOptimal;
        }

        ///<inheritdoc/>
        public AIWeaponBehaviour(AIEntity entity) : base(entity)
        {
        }

        ///<inheritdoc/>
        public override bool CanDo()
        {
            return true;
        }

        ///<inheritdoc/>
        public override void DoAction()
        {
            AIController controller = (AIController)entity;
            if (!controller)
                return;

            dist = Vector3.Distance(entity.Data.lastSeenPlayerPosition, entity.transform.position);

            controller.RotateTo((entity.Data.lastSeenPlayerPosition - entity.transform.position).normalized);

            if (IsDistanceOptimal())
            {
                if (controller.PunchWhenClose)
                {
                    if (dist <= controller.PunchDistance)
                    {
                        if (controller.CanPunch())
                            controller.Punch(Player.Instance.Damageable());
                    }
                    else
                    {
                        FireWeapon(controller);
                    }
                }
                else
                {
                    FireWeapon(controller);
                }
            }
            else if (controller.NavMeshAgent.velocity.magnitude <= 0.1f)
            {
                ThrowGrenade(controller);
            }
        }
    }
}