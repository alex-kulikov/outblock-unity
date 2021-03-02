using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Obsolete.
    /// </summary>
    public partial class Saveable : MonoBehaviour, ISaveable
    {

        public int Id { get; set; } = -1;
        public GameObject GO => gameObject;

        private Component[] components;

        private void OnDestroy()
        {
            Unregister();
        }

        public void Register()
        {
            components = gameObject.GetComponents(typeof(Component));
            SaveLoad.Add(this);
        }

        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            ExtendedSaveData saveData = (ExtendedSaveData)data;
            if (saveData != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] is Behaviour behaviour)
                    {
                        behaviour.enabled = saveData.ComponentDatas[i].Enabled;

                        if (behaviour is AudioSource source)
                        {
                            ExtendedSaveData.AudioSourceData audioSourceData = (ExtendedSaveData.AudioSourceData)saveData.ComponentDatas[i];
                            source.clip = audioSourceData.Clip;
                            if (audioSourceData.Playing)
                                source.Play();
                        }
                        else if (behaviour is Animator animator)
                        {
                            ExtendedSaveData.AnimatorData animatorData = (ExtendedSaveData.AnimatorData)saveData.ComponentDatas[i];
                            animator.speed = animatorData.Speed;
                            for(int l = 0; l < animator.layerCount; l++)
                            {
                                animator.Play(animatorData.States[l].fullPathHash, l, animatorData.States[l].normalizedTime);
                            }
                        }
                    }
                    else
                    {
                        switch (components[i])
                        {
                            case Rigidbody rb:
                                ExtendedSaveData.RigidbodyData rigidbodyData = (ExtendedSaveData.RigidbodyData)saveData.ComponentDatas[i];
                                rb.mass = rigidbodyData.Mass;
                                rb.isKinematic = rigidbodyData.IsKinematic;
                                rb.useGravity = rigidbodyData.UseGravity;
                                break;

                            case Collider cl:
                                ExtendedSaveData.ColliderData colliderData = (ExtendedSaveData.ColliderData)saveData.ComponentDatas[i];
                                cl.isTrigger = colliderData.IsTrigger;
                                cl.enabled = colliderData.Enabled;
                                break;

                            case MeshRenderer mr:
                                ExtendedSaveData.MeshRendererData meshRendererData = (ExtendedSaveData.MeshRendererData)saveData.ComponentDatas[i];
                                mr.materials = meshRendererData.Materials;
                                mr.enabled = meshRendererData.Enabled;
                                break;
                        }
                    }
                }
            }
        }

        public SaveData Save()
        {
            List<ExtendedSaveData.ComponentData> componentDatas = new List<ExtendedSaveData.ComponentData>();
            foreach(Component component in components)
            {
                switch (component)
                {
                    case AudioSource src:
                        componentDatas.Add(new ExtendedSaveData.AudioSourceData(src.enabled, src.clip, src.isPlaying));
                        break;

                    case Rigidbody rb:
                        componentDatas.Add(new ExtendedSaveData.RigidbodyData(true, rb.mass, rb.isKinematic, rb.useGravity));
                        break;

                    case Collider cl:
                        componentDatas.Add(new ExtendedSaveData.ColliderData(cl.enabled, cl.isTrigger));
                        break;

                    case MeshRenderer mr:
                        componentDatas.Add(new ExtendedSaveData.MeshRendererData(mr.enabled, mr.materials));
                        break;

                    case Animator am:
                        AnimatorStateInfo[] states = new AnimatorStateInfo[am.layerCount];
                        for (int i = 0; i < am.layerCount; i++)
                        {
                            states[i] = am.GetCurrentAnimatorStateInfo(i);
                        }
                        componentDatas.Add(new ExtendedSaveData.AnimatorData(am.enabled, am.speed, states));
                        break;

                    default:
                        if (component is Behaviour behaviour)
                            componentDatas.Add(new ExtendedSaveData.ComponentData(behaviour.enabled));
                        else componentDatas.Add(new ExtendedSaveData.ComponentData(true));
                        break;
                }
            }

            return new ExtendedSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, componentDatas);
        }
        
    }
}