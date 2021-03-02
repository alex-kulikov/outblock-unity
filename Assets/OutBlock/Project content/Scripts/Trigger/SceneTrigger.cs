using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OutBlock
{

    /// <summary>
    /// Changes scene.
    /// </summary>
    public class SceneTrigger : Trigger
    {

        [SerializeField, Header("Scene trigger")]
        private Object scene = default;
        [SerializeField, HideInInspector]
        private string sceneName;

        private void OnValidate()
        {
            if (scene != null)
                sceneName = scene.name;
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);

#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(UnityEditor.AssetDatabase.GetAssetPath(scene), new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadSceneAsync(sceneName);
#endif
        }

    }
}