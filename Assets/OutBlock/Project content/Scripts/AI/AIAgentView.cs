using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OutBlock
{
    
    /// <summary>
    /// Display for controller current state
    /// </summary>
    public class AIAgentView : MonoBehaviour
    {

        [SerializeField]
        private AIController controller = null;
        [SerializeField, Tooltip("Link to debug text")]
        private Text text = null;
        [SerializeField]
        private GameObject fillObject = null;
        [SerializeField]
        private Image fill = null;
        [SerializeField]
        private GameObject current = null;
        [SerializeField]
        private GameObject spottingHolder = null;
        [SerializeField]
        private Image spottingFill = null;

        private Transform cam;

        private void Update()
        {
            if (controller == null || controller.Data.currentState == null)
            {
                text.text = "";
                current.SetActive(false);
                fillObject.SetActive(false);

                return;
            }

            text.text = controller.Data.currentState.DebugStatus;

            fillObject.SetActive(controller.Sleeping);
            if (controller.Sleeping)
                fill.fillAmount = controller.sleepState.TimerRatio;

            if (!cam)
            {
                cam = Camera.main.transform;
            }
            else
            {
                transform.LookAt(cam);
            }

            current.SetActive(controller == GameUI.Instance().lastEnemy);

            spottingHolder.SetActive(controller.GetState() == controller.idleState && controller.Data.currentPlayerSpotWaitTime < controller.Data.PlayerSpotWaitTime);
            if (spottingHolder.activeSelf)
            {
                spottingFill.fillAmount = 1f - controller.Data.currentPlayerSpotWaitTime / controller.Data.PlayerSpotWaitTime;
            }
        }
    }
}