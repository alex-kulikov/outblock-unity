using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    public class AnimatorTrigger : Trigger
    {

        [System.Serializable]
        public class AnimatorAction
        {

            public enum ActionTypes { StopPlayback, StartPlayback, Play, CrossFade, SetSpeed, SetParameter};
            public enum ParameterTypes { Int, Float, Bool, Trigger };

            [SerializeField]
            private ActionTypes actionType = default;
            [SerializeField, Tooltip("State to play")]
            private string stateName = "";
            [SerializeField]
            private float transitionTime = 1;
            [SerializeField, Tooltip("Playback speed multiplier")]
            private float speed = 1;
            [SerializeField]
            private string parameterName = "";
            [SerializeField]
            private ParameterTypes parameterType;
            [SerializeField]
            private int valueInt = 0;
            [SerializeField]
            private float valueFloat = 0;
            [SerializeField]
            private bool valueBool = false;

            public void InitiateAction(Animator animator)
            {
                string layerName;
                switch (actionType)
                {
                    case ActionTypes.StopPlayback:
                        animator.StopPlayback();
                        break;

                    case ActionTypes.StartPlayback:
                        animator.StartPlayback();
                        break;

                    case ActionTypes.Play:
                        layerName = animator.GetLayerName(0);
                        animator.Play(layerName + "." + stateName);
                        break;

                    case ActionTypes.CrossFade:
                        layerName = animator.GetLayerName(0);
                        animator.CrossFade(layerName + "." + stateName, transitionTime);
                        break;

                    case ActionTypes.SetSpeed:
                        animator.speed = speed;
                        break;

                    case ActionTypes.SetParameter:
                        switch (parameterType)
                        {
                            case ParameterTypes.Int:
                                animator.SetInteger(parameterName, valueInt);
                                break;

                            case ParameterTypes.Float:
                                animator.SetFloat(parameterName, valueFloat);
                                break;

                            case ParameterTypes.Bool:
                                animator.SetBool(parameterName, valueBool);
                                break;

                            case ParameterTypes.Trigger:
                                animator.SetTrigger(parameterName);
                                break;
                        }
                        break;
                }
            }

        }

        [SerializeField, Header("AnimatorTrigger")]
        private AnimatorAction[] animatorActions = new AnimatorAction[0];

        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            Animator animator = other.GetComponentInChildren<Animator>();
            if (animator)
            {
                foreach(AnimatorAction action in animatorActions)
                {
                    action.InitiateAction(animator);
                }
            }
        }

    }
}