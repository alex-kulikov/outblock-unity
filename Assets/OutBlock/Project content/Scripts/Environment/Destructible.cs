using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// This class allows you to create destructible objects on level.
    /// </summary>
    public class Destructible : MonoBehaviour, IDamageable, ISaveable
    {

        [SerializeField]
        private float maxHP = 50;
        [SerializeField]
        private Transform deathPrefab = null;
        [SerializeField]
        private UnityEvent deathEvent = default;

        public event OnDeath onDeath = default;

        ///<inheritdoc/>
        public bool Sleeping => false;
        ///<inheritdoc/>
        public bool RagdollEnabled => false;
        ///<inheritdoc/>
        public float HPRatio => HP / maxHP;

        private float hp;
        private float HP
        {
            get
            {
                return hp;
            }

            set
            {
                hp = value;

                if (hp <= 0)
                {
                    hp = 0;
                    Kill();
                }
            }
        }

        public bool Dead { get; private set; }
        public int Id { get; set; } = -1;

        public GameObject GO => gameObject;

        private void Start()
        {
            HP = maxHP;
        }

        private void OnDestroy()
        {
            Unregister();
        }

        /// <inheritdoc/>
        public void Kill()
        {
            if (Dead)
                return;

            Dead = true;

            if (deathPrefab)
                Instantiate(deathPrefab, transform.position, transform.rotation);

            onDeath?.Invoke(this);
            deathEvent?.Invoke();

            gameObject.SetActive(false);
        }

        ///<inheritdoc/>
        public void Revive()
        {
            if (!Dead)
                return;

            Dead = false;

            gameObject.SetActive(true);
        }

        ///<inheritdoc/>
        public void Damage(DamageInfo damageInfo)
        {
            HP -= damageInfo.damage;
        }

        ///<inheritdoc/>
        public IDamageable Damageable()
        {
            return this;
        }

        ///<inheritdoc/>
        public Vector3 GetTargetPos()
        {
            return transform.position;
        }

        #region SaveLoad
        public void Register()
        {
            SaveLoad.Add(this);
        }

        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public SaveData Save()
        {
            float hp = this.hp;
            if (!Dead && hp == 0)
                hp = maxHP;
            return new DamageableSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, hp, Dead);
        }

        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            DamageableSaveData saveData = (DamageableSaveData)data;
            if (saveData != null)
            {
                HP = saveData.hp;
                Dead = saveData.dead;
            }
        }
        #endregion
    }

}