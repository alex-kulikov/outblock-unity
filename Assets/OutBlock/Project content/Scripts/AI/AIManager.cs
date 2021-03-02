using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OutBlock
{

    /// <summary>
    /// 
    /// </summary>
    public class AIManager : MonoBehaviour
    {

        [SerializeField]
        private int maxBotsAttacking = 5;
        [SerializeField]
        private int closestBotAttacking = 2;
        [SerializeField]
        private float attackingUpdateRate = 15;
        [SerializeField, Range(0, 100)]
        private int otherBotsAttackChance = 0;
        [SerializeField, Space]
        private int maxGrenades = 1;
        [SerializeField]
        private float grenadeCooldown = 5;

        /// <summary>
        /// Closest AI entity to the player
        /// </summary>
        public static AIEntity closestToPlayer;

        private static AIManager instance;
        private static List<AIEntity> ais = new List<AIEntity>();
        private static List<bool> aisAttack = new List<bool>();
        private static List<float> grenadesQueue = new List<float>();
        private const bool checkVisibility = true;

        /// <summary>
        /// Instance of the AI Manager. Only one instance of the AIManager class can be on scene!
        /// </summary>
        /// <param name="onDestroy">Set true when calling on OnDisable/OnDestroy to prevent error while unloading scene or game</param>
        public static AIManager Instance(bool onDestroy = false)
        {
            if (instance == null)
            {
                if (onDestroy)
                    return null;

                AIManager inScene = FindObjectOfType<AIManager>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    GameObject go = new GameObject("AIManager");
                    AIManager aIManager = go.AddComponent<AIManager>();
                    instance = aIManager;
                }
            }
            return instance;
        }

        private void Awake()
        {
            InvokeRepeating("CheckForClosest", 0.1f, 0.5f);
            InvokeRepeating("UpdateAttackOrder", 0.2f, attackingUpdateRate);
        }

        private void OnDestroy()
        {
            ais.Clear();
        }

        private void Update()
        {
            for(int i = grenadesQueue.Count - 1; i >= 0; i--)
            {
                grenadesQueue[i] -= Time.deltaTime;
                if (grenadesQueue[i] <= 0)
                    grenadesQueue.RemoveAt(i);
            }
        }

        private void UpdateAttackOrder()
        {
            aisAttack.Clear();

            int n = 0;
            for (int i = 0; i < ais.Capacity; i++)
            {
                if (i >= ais.Count)
                {
                    aisAttack.Add(false);
                    continue;
                }

                if (!ais[i].CanAttack)
                {
                    aisAttack.Add(false);
                    continue;
                }

                if (!ais[i].gameObject.activeSelf)
                {
                    aisAttack.Add(false);
                    continue;
                }

                if (ais[i].IgnoreAttackOrder)
                {
                    aisAttack.Add(true);
                    continue;
                }

                if (n < closestBotAttacking)
                {
                    aisAttack.Add(true);
                    n++;
                }
                else
                {
                    if (n < maxBotsAttacking || Utils.CalculateChance(otherBotsAttackChance))
                    {
                        aisAttack.Add(true);
                        n++;
                    }
                    else
                    {
                        aisAttack.Add(false);
                    }
                }
            }
        }

        private void SortAIsByDistance()
        {
            ais = ais.OrderBy(
                x => Vector3.Distance(Player.Instance.transform.position, x.transform.position)
            ).ToList();
        }

        private void CheckForClosest()
        {
            if (Player.Instance == null)
                return;

            if (ais.Count == 0)
                return;

            SortAIsByDistance();

            int index = 0;
            if (checkVisibility)
            {
                index = -1;
                LayerMask mask = LayerMask.GetMask("Default");

                for (int i = 0; i < ais.Count; i++)
                {
                    if (!Physics.Linecast(Player.Instance.GetTargetPos(), ais[i].GetTargetPos(), mask))
                    {
                        index = i;
                        break;
                    }
                }

                closestToPlayer = index >= 0 ? ais[index] : null;
            }
        }

        /// <summary>
        /// Can entity use grenade?
        /// </summary>
        public bool CanThrow()
        {
            return grenadesQueue.Count < maxGrenades;
        }

        /// <summary>
        /// Enqueu grenade queue
        /// </summary>
        public void Throw()
        {
            grenadesQueue.Add(grenadeCooldown);
        }

        /// <summary>
        /// Can the entity use a weapon?
        /// </summary>
        public bool CanFire(AIEntity entity)
        {
            return aisAttack[GetIndex(entity)];
        }

        /// <summary>
        /// Index of the entity
        /// </summary>
        public int GetIndex(AIEntity entity)
        {
            return ais.IndexOf(entity);
        }

        /// <summary>
        /// Register the entity in the AIManager and assign the AIZone for it based on its position.
        /// </summary>
        public void RegisterAI(AIEntity ai)
        {
            ais.Add(ai);

            AIZone zone = AIZone.GetZoneByPos(ai.transform.position);
            if (zone == null)
            {
                zone = AIZone.CreateSceneSizeZone();
            }

            zone.AddAI(ai);

            UpdateAttackOrder();
        }

        /// <summary>
        /// Remove the entity from the AIManager
        /// </summary>
        public void DisposeAI(AIEntity ai)
        {
            ais.Remove(ai);
            if (ai.assignedZone)
            {
                ai.assignedZone.RemoveAI(ai);
                ai.assignedZone = null;
            }
        }

        public enum States { Idle, Attention, Aggresion };

        /// <summary>
        /// Get current entities state
        /// </summary>
        /// <param name="cooldown">Current state cooldown. Range from 0 to 1</param>
        public States GetAIStatus(out float cooldown)
        {
            States result = States.Idle;
            float attentionCooldown, aggresionCooldown;
            attentionCooldown = aggresionCooldown = cooldown = 0;

            for (int i = 0; i < ais.Count; i++)
            {
                if (ais[i].GetState() is AIStateAttention)
                {
                    result = States.Attention;
                    AIStateAttention attention = (AIStateAttention)ais[i].GetState();
                    if (attention.Cooldown > attentionCooldown)
                        attentionCooldown = attention.Cooldown;
                }
                else if (ais[i].GetState() is AIStateAggresion)
                {
                    result = States.Aggresion;
                    AIStateAggresion aggresion = (AIStateAggresion)ais[i].GetState();
                    if (aggresion.Cooldown > aggresionCooldown)
                        aggresionCooldown = aggresion.Cooldown;
                }
                else if (ais[i].GetState() is AIStateCover)
                {
                    result = States.Aggresion;
                    aggresionCooldown = 1;
                }
            }

            AIZone[] zones = AIZone.GetZones();
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i].IsAggression())
                {
                    result = States.Aggresion;
                    break;
                }
            }

            if (result == States.Attention)
                cooldown = attentionCooldown;
            else if (result == States.Aggresion)
                cooldown = aggresionCooldown;

            return result;
        }

#if UNITY_EDITOR
        #region Menu items
        [MenuItem("OutBlock/AI/Add AI Zone")]
        public static void AddAIZone()
        {
            GameObject go = new GameObject("AIZone");
            go.layer = 12;
            AIZone zone = go.AddComponent<AIZone>();
            BoxCollider coll = go.AddComponent<BoxCollider>();
            coll.size = Vector3.one * 5;
            coll.isTrigger = true;
            Selection.activeGameObject = go;
        }

        [MenuItem("OutBlock/AI/Add Patrol Path")]
        public static void AddPatrolPath()
        {
            GameObject go = new GameObject("PatrolPath");
            go.AddComponent<AIPath>();
            Selection.activeGameObject = go;
        }

        [MenuItem("OutBlock/AI/Add AI Manager")]
        public static void AddAIManager()
        {
            AIManager inScene = FindObjectOfType<AIManager>();
            if (!inScene)
            {
                GameObject go = new GameObject("AIManager");
                go.AddComponent<AIManager>();
                Selection.activeGameObject = go;
            }
            else
            {
                Selection.activeGameObject = inScene.gameObject;
            }
        }
        #endregion
#endif
    }
}