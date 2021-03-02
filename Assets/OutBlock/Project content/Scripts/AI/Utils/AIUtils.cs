using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Utility functions for AI
    /// </summary>
    public static class AIUtils
    {
        
        /// <summary>
        /// Try to scare the entity if conditions are true. This will make entity run away from fear point.
        /// Conditions: entity is not already scared; if dist is greater than 5.
        /// </summary>
        /// <param name="entity">Target entity</param>
        /// <param name="fearPoint">Source of fear</param>
        public static void InitiateFear(AIEntity entity, Transform fearPoint)
        {
            if (!(entity is AIController))
                return;

            if (entity.InFear)
                return;

            AIController controller = (AIController)entity;
            float dist = Vector3.Distance(entity.transform.position, fearPoint.position);

            if (controller.GetState() is AIStateIdle)
                controller.Aggression();
            else if (controller.GetState() is AIStateCover)
                controller.Aggression();

            if (dist <= 5)
            {
                entity.Data.fearTimer = entity.Data.FearCooldownTime;
                controller.ForceMoveTo(entity.transform.position + (entity.transform.position - fearPoint.position).normalized * 5);
            }
        }

    }
}