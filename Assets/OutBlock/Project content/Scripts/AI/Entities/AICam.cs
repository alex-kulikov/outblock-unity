using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Entity for the AI camera
    /// </summary>
    public class AICam : AIEntity
    {

        [SerializeField, Header("Stats")]
        private float maxHP = 100;
        [SerializeField, Tooltip("Camera view update delay")]
        private float updateRate = 1;
        [SerializeField, Tooltip("Time before camera will alarm ai")]
        private float alarmTime = 1;

        private bool playerVisible;
        private float timeout;
        private float hp;

        ///<inheritdoc/>
        public override bool CanAttack => false;
        ///<inheritdoc/>
        public override bool IgnoreAttackOrder => false;
        ///<inheritdoc/>
        public override bool RagdollEnabled => false;
        ///<inheritdoc/>
        public override int Id { get; set; } = -1;
        ///<inheritdoc/>
        public override Transform AssignedTransform => transform;
        ///<inheritdoc/>
        public override bool Sleeping => false;
        ///<inheritdoc/>
        public override float HPRatio => hp / maxHP;
        ///<inheritdoc/>
        public override GameObject GO => gameObject;
        ///<inheritdoc/>
        public override event OnDeath onDeath;
        ///<inheritdoc/>
        public override bool Dead { get; protected set; }

        private void Start()
        {
            hp = maxHP;

            data.currentState = new AIStateAggresion();
            data.currentState.Init(this);
            Invoke("CheckVision", updateRate);
        }

        private void OnEnable()
        {
            AIManager.Instance().RegisterAI(this);

            sensors.OnHearSound += Sensors_OnHearSound;
        }

        private void OnDisable()
        {
            AIManager.Instance(true)?.DisposeAI(this);

            sensors.OnHearSound -= Sensors_OnHearSound;
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Update()
        {
            if (timeout > 0)
            {
                timeout -= Time.deltaTime;
            }
            else if (playerVisible && Player.Instance)
            {
                assignedZone.ForceAggresion();
                assignedZone.PropogatePlayerPosition(Player.Instance.transform.position);
            }
        }

        private void CheckVision()
        {
            if (Player.Instance == null)
            {
                playerVisible = false;
                Invoke("CheckVision", playerVisible ? Time.fixedDeltaTime : updateRate);
                return;
            }

            AISensors.VisionResult visionResult = sensors.CheckVision();
            if (!playerVisible && visionResult.playerVisible)
            {
                timeout = alarmTime;
            }
            playerVisible = visionResult.playerVisible;
            Invoke("CheckVision", playerVisible ? Time.fixedDeltaTime : updateRate);
        }

        private void Sensors_OnHearSound(SoundStimuli obj)
        {
            assignedZone.ForceAttention();
            assignedZone.PropogatePlayerPosition(obj.caster.position);
        }

        ///<inheritdoc/>
        public override AIState GetState()
        {
            return playerVisible ? data.currentState : null;
        }

        ///<inheritdoc/>
        public override void Aggression()
        {
        }

        ///<inheritdoc/>
        public override void Attention()
        {
        }

        ///<inheritdoc/>
        public override IDamageable Damageable()
        {
            return null;
        }

        ///<inheritdoc/>
        public override void Damage(DamageInfo damageInfo)
        {
            if (damageInfo.team == Teams.AI && !friendlyFire)
                return;

            if (damageInfo.damageType == DamageInfo.DamageTypes.Sleep)
                return;

            hp -= damageInfo.damage;
            if (hp <= 0)
                Kill();
        }

        ///<inheritdoc/>
        public override void Kill()
        {
            if (Dead)
                return;

            Dead = true;

            events.OnDeath?.Invoke();
            onDeath?.Invoke(this);
            gameObject.SetActive(false);
        }

        ///<inheritdoc/>
        public override void Revive()
        {
            if (!Dead)
                return;

            Dead = false;
            gameObject.SetActive(true);
        }

        ///<inheritdoc/>
        public override Vector3 GetTargetPos()
        {
            return transform.position + Vector3.up;
        }

        #region Saveable
        /// <inheritdoc/>
        public override void Register()
        {
            SaveLoad.Add(this);
        }

        /// <inheritdoc/>
        public override void Unregister()
        {
            SaveLoad.Remove(this);
        }

        ///<inheritdoc/>
        public override SaveData Save()
        {
            float hp = this.hp;
            if (!Dead && hp == 0)
                hp = maxHP;
            return new EntitySaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, hp, Dead, null);
        }

        ///<inheritdoc/>
        public override void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            EntitySaveData entitySaveData = (EntitySaveData)data;
            if (entitySaveData != null)
            {
                hp = entitySaveData.hp;
                if (Dead != entitySaveData.dead)
                {
                    if (entitySaveData.dead)
                        Kill();
                    else Revive();

                    Dead = entitySaveData.dead;
                }
            }
        }
        #endregion
    }
}