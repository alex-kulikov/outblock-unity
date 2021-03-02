using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace OutBlock
{

    /// <summary>
    /// Menu UI.
    /// </summary>
    public class Menu : MonoBehaviour
    {

        [SerializeField]
        private Object scene = default;
        [SerializeField, HideInInspector]
        private string sceneName;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnValidate()
        {
            if (scene != null)
            {
                sceneName = scene.name;
            }
        }

        private IEnumerator SelectObjectSequence(GameObject go)
        {
            yield return 0;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }

        /// <summary>
        /// Load game scene.
        /// </summary>
        public void Play()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(UnityEditor.AssetDatabase.GetAssetPath(scene), new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadSceneAsync(sceneName);
#endif
        }

        /// <summary>
        /// Play last scene again. Use in the victory scene.
        /// </summary>
        public void PlayAgain()
        {
            if (!string.IsNullOrEmpty(VictoryTrigger.PlayAgainScene))
            {
#if UNITY_EDITOR
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(VictoryTrigger.PlayAgainScene, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadSceneAsync(VictoryTrigger.PlayAgainScene);
#endif
            }
            else Play();
        }

        /// <summary>
        /// Quit the game.
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Select UI element.
        /// </summary>
        public void SelectObject(GameObject go)
        {
            StartCoroutine(SelectObjectSequence(go));
        }

    }
}