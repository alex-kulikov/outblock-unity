using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;

namespace OutBlock
{

    /// <summary>
    /// Responsible for the timeline logic.
    /// </summary>
    public class TimelineManager : MonoBehaviour
    {

        [SerializeField]
        private PlayableAsset[] playableAssets = new PlayableAsset[0];
        [SerializeField]
        private bool playOnAwake = false;

        private PlayableDirector director;
        private int step;

        public delegate void OnStart();
        /// <summary>
        /// Timeline started to play.
        /// </summary>
        public event OnStart onStart;

        public delegate void OnEnd();
        /// <summary>
        /// Timeline is stopped.
        /// </summary>
        public event OnEnd onEnd;

        private GameObject cam;

        /// <summary>
        /// Is any timeline playing right now?
        /// </summary>
        public static bool playing { get; private set; }

        private static TimelineManager instance;
        /// <summary>
        /// TimelineManager instance. Only one on the scene.
        /// </summary>
        public static TimelineManager Instance(bool onExit = false)
        {
            if (instance == null)
            {
                if (onExit)
                    return null;

                TimelineManager inScene = FindObjectOfType<TimelineManager>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    GameObject go = new GameObject("TimelineManager");
                    TimelineManager timelineManager = go.AddComponent<TimelineManager>();
                    instance = timelineManager;
                }
            }
            return instance;
        }

        private void Awake()
        {
            playing = false;
            director = GetComponent<PlayableDirector>();
            if (director == null)
                director = gameObject.AddComponent<PlayableDirector>();
            step = -1;
            if (playOnAwake)
                NextStep();
            if (transform.childCount > 0)
                cam = transform.GetChild(0).gameObject;
        }

        private void Update()
        {
            if (director.state != PlayState.Playing && playing)
                Stop();

            if (cam && cam.activeSelf && !playing)
                cam.SetActive(false);
        }

        private void Play()
        {
            director.Stop();
            director.playableAsset = playableAssets[step];
            director.Play();
            playing = true;
            onStart?.Invoke();
        }

        private void Stop()
        {
            playing = false;
            onEnd?.Invoke();
        }

        /// <summary>
        /// Get current timeline index.
        /// </summary>
        public int GetStep()
        {
            return step;
        }

        /// <summary>
        /// Select the next timeline and play it.
        /// </summary>
        public void NextStep()
        {
            step++;

            if (step < playableAssets.Length)
            {
                Play();
            }
        }

        /// <summary>
        /// Replay the timeline.
        /// </summary>
        public void Replay()
        {
            Play();
        }

        /// <summary>
        /// Select the timeline.
        /// </summary>
        public void SetStep(int index)
        {
            if (step >= index)
                return;

            step = index;

            if (step < playableAssets.Length)
            {
                Play();
            }
        }

        /// <summary>
        /// Skip currently playing timeline.
        /// </summary>
        public void SkipScene()
        {
            director.time = director.playableAsset.duration - 0.1;
        }

#if UNITY_EDITOR
        [MenuItem("OutBlock/Timeline/Add Timeline Manager")]
        private static void AddToTheScene()
        {
            TimelineManager manager = FindObjectOfType<TimelineManager>();
            if (manager)
            {
                EditorUtility.DisplayDialog("Timeline manager", "Timeline manager is already in the scene", "OK");
            }
            else
            {
                Object mng = PrefabUtility.InstantiatePrefab(Resources.Load<TimelineManager>("TimelineManager"));
                Selection.activeObject = mng;
                Undo.RegisterCreatedObjectUndo(mng, "");
            }
        }
#endif
    }
}