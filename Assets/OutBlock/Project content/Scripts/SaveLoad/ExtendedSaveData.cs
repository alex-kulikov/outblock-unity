using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    public partial class Saveable
    {
        public class ExtendedSaveData : SaveData
        {

            public class ComponentData
            {
                public bool Enabled { get; private set; }

                public ComponentData(bool enabled)
                {
                    Enabled = enabled;
                }
            }

            public class RigidbodyData : ComponentData
            {
                public float Mass { get; private set; }
                public bool IsKinematic { get; private set; }
                public bool UseGravity { get; private set; }

                public RigidbodyData(bool enabled, float mass, bool isKinematic, bool useGravity) : base(enabled)
                {
                    Mass = mass;
                    IsKinematic = isKinematic;
                    UseGravity = useGravity;
                }
            }

            public class MeshRendererData : ComponentData
            {
                public Material[] Materials { get; private set; }

                public MeshRendererData(bool enabled, Material[] materials) : base(enabled)
                {
                    Materials = materials;
                }
            }

            public class AudioSourceData : ComponentData
            {
                public AudioClip Clip { get; private set; }
                public bool Playing { get; private set; }

                public AudioSourceData(bool enabled, AudioClip clip, bool playing) : base(enabled)
                {
                    Clip = clip;
                    Playing = playing;
                }
            }

            public class ColliderData : ComponentData
            {
                public bool IsTrigger { get; private set; }

                public ColliderData(bool enabled, bool isTrigger) : base(enabled)
                {
                    IsTrigger = isTrigger;
                }
            }

            public class AnimatorData : ComponentData
            {
                public float Speed { get; private set; }
                public AnimatorStateInfo[] States { get; private set; }

                public AnimatorData(bool enabled, float speed, AnimatorStateInfo[] states) : base(enabled)
                {
                    Speed = speed;
                    States = states;
                }
            }

            public List<ComponentData> ComponentDatas { get; private set; }

            public ExtendedSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, List<ComponentData> componentDatas) : base(id, pos, rot, active, enabled)
            {
                ComponentDatas = componentDatas;
            }
        }
        
    }
}