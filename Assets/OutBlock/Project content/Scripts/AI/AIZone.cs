using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// AI zone for the entities
    /// </summary>
    public class AIZone : MonoBehaviour
    {

        /// <summary>
        /// Assigned entities to the zone
        /// </summary>
        public List<AIEntity> assignedAIs { get; private set; } = new List<AIEntity>();

        private float aggressionTime = 0;
        private Dictionary<AIEntity, Vector3> moves = new Dictionary<AIEntity, Vector3>();

        private static List<AIZone> zones = new List<AIZone>();

        private const float alarmCooldown = 0.5f;

        private void OnEnable()
        {
            gameObject.layer = 12;
            zones.Add(this);
        }

        private void OnDisable()
        {
            zones.Remove(this);
        }

        private void Update()
        {
            if (aggressionTime > 0)
                aggressionTime -= Time.deltaTime / alarmCooldown;
        }

        private bool SweepPoint(Vector3 pos)
        {
            Vector3 dir = (transform.position - pos).normalized;
            Vector3 start = pos - dir * 1000;
            int intersections = 0;
            Vector3 point = start;
            LayerMask layerMask = LayerMask.GetMask("AIZone");

            while (point != pos)
            {
                if (Physics.Linecast(point, pos, out RaycastHit hit, layerMask))
                {
                    if (hit.transform == transform)
                        intersections++;
                    point = hit.point + dir * 0.1f;
                }
                else
                {
                    point = pos;
                }
            }
            while (point != start)
            {
                if (Physics.Linecast(point, start, out RaycastHit hit, layerMask))
                {
                    if (hit.transform == transform)
                        intersections++;
                    point = hit.point - dir * 0.1f;
                }
                else
                {
                    point = start;
                }
            }

            return intersections % 2 == 1;
        }

        /// <summary>
        /// Clear last move of the entity
        /// </summary>
        public void ClearMove(AIEntity entity)
        {
            moves.Remove(entity);
        }

        /// <summary>
        /// Add move of the entity. This needs for the correct distancing of the entities.
        /// </summary>
        public void AddMove(AIEntity entity, Vector3 point)
        {
            if (moves.ContainsKey(entity))
            {
                moves[entity] = point;
            }
            else
            {
                moves.Add(entity, point);
            }
        }

        /// <summary>
        /// Check if the move of the entity is valid taking in the account the distance between the entities
        /// </summary>
        /// <param name="minDist">Minimal distance between the entities</param>
        public bool ValidateMove(AIEntity entity, Vector3 point, float minDist)
        {
            if (moves.Count <= 0)
                return true;

            foreach(KeyValuePair<AIEntity, Vector3> pair in moves)
            {
                if (pair.Key == entity)
                    continue;

                if (Vector3.Distance(point, pair.Value) < minDist)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add entity to the zone
        /// </summary>
        /// <param name="ai"></param>
        public void AddAI(AIEntity ai)
        {
            assignedAIs.Add(ai);
            ai.assignedZone = this;
        }

        /// <summary>
        /// Remove entity from the zone
        /// </summary>
        /// <param name="ai"></param>
        public void RemoveAI(AIEntity ai)
        {
            assignedAIs.Remove(ai);
            ClearMove(ai);
        }

        /// <summary>
        /// Check if the entity is in the zone
        /// </summary>
        /// <param name="ai"></param>
        /// <returns></returns>
        public bool IsAIInZone(AIEntity ai)
        {
            return assignedAIs.Contains(ai);
        }

        /// <summary>
        /// Check if zone in the aggression state
        /// </summary>
        public bool IsAggression()
        {
            return aggressionTime > 0;
        }

        /// <summary>
        /// Switch all entities to the aggression state ignoring the distance
        /// </summary>
        public void ForceAggresion()
        {
            aggressionTime = 1;
            foreach (AIEntity ai in assignedAIs)
            {
                ai.Aggression();
            }
        }

        /// <summary>
        /// Switch all entities to the attention state ignoring the distance
        /// </summary>
        public void ForceAttention()
        {
            foreach (AIEntity ai in assignedAIs)
            {
                ai.Attention();
            }
        }

        /// <summary>
        /// Switch all entities to the aggression state if they close
        /// </summary>
        public void Aggresion(Vector3 entityPos)
        {
            aggressionTime = 1;
            foreach (AIEntity ai in assignedAIs)
            {
                float dist = Vector3.Distance(entityPos, ai.transform.position) / 2;
                if (dist <= ai.Sensors.HearingRadius)
                    ai.Aggression();
            }
        }

        /// <summary>
        /// Switch all entities to the attention state if they close
        /// </summary>
        public void Attention(Vector3 entityPos)
        {
            foreach (AIEntity ai in assignedAIs)
            {
                float dist = Vector3.Distance(entityPos, ai.transform.position) / 2;
                if (dist <= ai.Sensors.HearingRadius)
                    ai.Attention();
            }
        }

        /// <summary>
        /// Switch all entities to the attention state and propagate player position
        /// </summary>
        /// <param name="pos">Player position</param>
        public void Attention(Vector3 entityPos, Vector3 pos)
        {
            PropogatePlayerPosition(pos);
            Attention(entityPos);
        }

        /// <summary>
        /// Propagate player position to the all entities in the zone
        /// </summary>
        public void PropogatePlayerPosition(Vector3 position)
        {
            foreach (AIEntity ai in assignedAIs)
            {
                ai.Data.lastSeenPlayerPosition = position;
            }
        }

        /// <summary>
        /// Send loud sound to the all entities in the zone
        /// </summary>
        public static void HearSound(SoundStimuli soundStimuli)
        {
            AIZone[] zones = GetZonesByPos(soundStimuli.caster.position);
            for (int i = 0; i < zones.Length; i++)
            {
                for (int j = 0; j < zones[i].assignedAIs.Count; j++)
                {
                    zones[i].assignedAIs[j].Sensors.HearSound(soundStimuli);
                }
            }
        }

        /// <summary>
        /// Get AIZone by the position
        /// </summary>
        public static AIZone GetZoneByPos(Vector3 pos)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].SweepPoint(pos))
                    return zones[i];
            }

            return null;
        }

        /// <summary>
        /// Get several AIZones by the position(if they are overlapping)
        /// </summary>
        public static AIZone[] GetZonesByPos(Vector3 pos)
        {
            List<AIZone> zns = new List<AIZone>();

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].SweepPoint(pos))
                    zns.Add(zones[i]);
            }

            return zns.ToArray();
        }

        /// <summary>
        /// Get all AIZones
        /// </summary>
        public static AIZone[] GetZones()
        {
            return zones.ToArray();
        }

        /// <summary>
        /// Creates an AIZone with size of the whole scene.
        /// </summary>
        public static AIZone CreateSceneSizeZone()
        {
            GameObject go = new GameObject("AutoAIZone");
            AIZone zone = go.AddComponent<AIZone>();
            go.layer = 12;
            BoxCollider coll = go.AddComponent<BoxCollider>();
            Bounds sceneBounds = Utils.GetSceneBounds();
            coll.center = sceneBounds.center;
            coll.size = sceneBounds.size;
            coll.isTrigger = true;

            return zone;
        }

    }
}