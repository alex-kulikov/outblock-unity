using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OutBlock
{
    
    /// <summary>
    /// This class stores settings for an entity
    /// </summary>
    public class AIAgentData : MonoBehaviour
    {
        //Settings
        [SerializeField, Header("Settings"), Tooltip("Path for Entity while in idle state")]
        private AIPath path = null;
        public AIPath Path { get => path; set => path = value; }
        [SerializeField, Tooltip("Time for transition from attention state to idle")]
        private float attentionWaitTime = 2f;
        public float AttentionWaitTime => attentionWaitTime;
        [SerializeField, Tooltip("Time for transition from aggresion state to idle")]
        private float aggresionWaitTime = 5f;
        public float AggresionWaitTime => aggresionWaitTime;
        [SerializeField, Tooltip("Time to become aggressive from attention state")]
        private float attentionPlayerSpotWaitTime = 0.8f;
        public float AttentionPlayerSpotWaitTime => attentionPlayerSpotWaitTime;
        [SerializeField, Tooltip("Time to spot the player from idle state")]
        private float playerSpotWaitTime = 1f;
        public float PlayerSpotWaitTime => playerSpotWaitTime;
        [SerializeField, Tooltip("Time range for transition to sleep from idle state")]
        private MinMaxFloat waitForSleepRange = default;
        public MinMaxFloat WaitForSleepRange => waitForSleepRange;
        [SerializeField, Tooltip("Sleep time range to wake up")]
        private MinMaxFloat sleepTimeRange = default;
        public MinMaxFloat SleepTimeRange => sleepTimeRange;
        [SerializeField, Tooltip("Radius for call for attention for over agents")]
        private float attentionRadius = 4f;
        public float AttentionRadius => attentionRadius;
        [SerializeField, Tooltip("Optimal radius for shooting in aggression state, agent is moving to this distance")]
        private float shootingDistanceOptimal = 4f;
        public float ShootingDistanceOptimal => shootingDistanceOptimal;
        [SerializeField, Tooltip("Fear cooldown time")]
        private float fearCooldownTime = 4f;
        public float FearCooldownTime => fearCooldownTime;

        //Runtime data

        /// <summary>
        /// Current state of controller
        /// </summary>
        public AIState currentState { get; set; }

        /// <summary>
        /// Last known player position, may be assigned by other AI's
        /// </summary>
        public Vector3 lastSeenPlayerPosition { get; set; }

        /// <summary>
        /// Timer for attention state
        /// </summary>
        public float currentAttentionWaitTime { get; set; }

        /// <summary>
        /// Timer for aggression state
        /// </summary>
        public float currentAggresionWaitTime { get; set; }

        /// <summary>
        /// Timer for become aggresive from attention
        /// </summary>
        public float currentAttentionPlayerSpotWaitTime { get; set; }

        /// <summary>
        /// Timer for spotting player in the idle state
        /// </summary>
        public float currentPlayerSpotWaitTime { get; set; }

        /// <summary>
        /// Point AI was activated
        /// </summary>
        public Vector3 spawnPoint { get; set; }

        /// <summary>
        /// Timer for waiting on point when patroling, timer is reseting to point's time
        /// </summary>
        public float pointWaitTime { get; set; }

        /// <summary>
        /// Timer for transition to sleep
        /// </summary>
        public float waitForSleep { get; set; }

        /// <summary>
        /// Timer for sleep state
        /// </summary>
        public float sleepTimer { get; set; }

        /// <summary>
        /// Timer for fear
        /// </summary>
        public float fearTimer { get; set; }

    }
}