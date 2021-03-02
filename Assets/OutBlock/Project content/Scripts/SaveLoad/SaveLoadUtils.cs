using UnityEngine;

namespace OutBlock
{
    public static class SaveLoadUtils
    {

        /// <summary>
        /// Basic load procedure.
        /// </summary>
        public static void BasicLoad(MonoBehaviour go, SaveData data)
        {
            go.gameObject.SetActive(data.active);
            go.gameObject.transform.position = data.pos;
            go.gameObject.transform.localEulerAngles = data.rot;
            go.enabled = data.enabled;
        }

    }
}
