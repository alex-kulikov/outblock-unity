using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OutBlock
{

    /// <summary>
    /// Additional functions.
    /// </summary>
    public static class Utils
    {

        private static AudioMixer mixer = null;
        /// <summary>
        /// Get current AudioMixer.
        /// </summary>
        public static AudioMixer Mixer
        {
            get
            {
                if (!mixer)
                {
                    mixer = Resources.Load<AudioMixer>("Mixer");
                }
                return mixer;
            }
        }

        /// <summary>
        /// Clamp the angle.
        /// </summary>
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < 90 || angle > 270)
            {
                if (angle > 180) angle -= 360;
                if (max > 180) max -= 360;
                if (min > 180) min -= 360;
            }
            angle = Mathf.Clamp(angle, min, max);
            if (angle < 0) angle += 360;
            return angle;
        }

        /// <summary>
        /// Lerp Vector3 angle.
        /// </summary>
        public static Vector3 AngleLerp(Vector3 a, Vector3 b, float t)
        {
            Vector3 result;
            result.x = Mathf.LerpAngle(a.x, b.x, t);
            result.y = Mathf.LerpAngle(a.y, b.y, t);
            result.z = Mathf.LerpAngle(a.z, b.z, t);
            return result;
        }

        /// <summary>
        /// Correct the angle to the range from 0 to 360.
        /// </summary>
        public static Vector3 CorrectAngle(Vector3 a)
        {
            Vector3 result = a;
            if (result.x < 0)
                result.x = 360 + result.x;
            if (result.y < 0)
                result.y = 360 + result.y;
            if (result.z < 0)
                result.z = 360 + result.z;

            return result;
        }

        /// <summary>
        /// Converts string to the Vector3.
        /// </summary>
        /// <param name="input">Input string. Correct format: "0.1, 0.1, 0.3". The comma splits vector values and the dot splits the left and right sides of the float values.</param>
        /// <returns></returns>
        public static Vector3 StringToVector3(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new Vector3();

            Vector3 result = new Vector3();
            string[] values = input.Split(',');

            if (values.Length <= 0)
                return result;

            for (int i = 0; i < values.Length; i++)
            {
                if (i > 2)
                    break;

                float value = float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
                switch (i)
                {
                    case 0:
                        result.x = value;
                        break;

                    case 1:
                        result.y = value;
                        break;

                    case 2:
                        result.z = value;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Roll the chance.
        /// </summary>
        /// <param name="chance">Possible chance from 0-100.</param>
        public static bool CalculateChance(int chance)
        {
            return Random.Range(1, 101) <= chance;
        }

        /// <summary>
        /// Compare multiple tags and return true if any of the tags are equal.
        /// </summary>
        public static bool CompareTags(Transform transform, params string[] tags)
        {
            foreach (string tag in tags)
            {
                if (transform.CompareTag(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Compare two <see cref="Ray"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool CompareRays(Ray a, Ray b)
        {
            return a.origin == b.origin && a.direction == b.direction;
        }

        /// <summary>
        /// Draw gizmo XYZ axis.
        /// </summary>
        public static void GizmosDrawAxis(Vector3 center, float size)
        {
            float extent = size / 2;
            Gizmos.DrawLine(center - Vector3.forward * extent, center + Vector3.forward * extent);
            Gizmos.DrawLine(center - Vector3.right * extent, center + Vector3.right * extent);
            Gizmos.DrawLine(center - Vector3.up * extent, center + Vector3.up * extent);
        }

        /// <summary>
        /// Draw gizmo circle.
        /// </summary>
        public static void GizmosDrawCircle(Vector3 position, float radius)
        {
            float theta = 0;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = position + new Vector3(x, 0, y);
            Vector3 newPos = pos;
            Vector3 lastPos = pos;
            for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = position + new Vector3(x, 0, y);
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }

        /// <summary>
        /// Play sound.
        /// </summary>
        public static void PlaySound(Vector3 pos, AudioClip clip, float vol, float pitch)
        {
            GameObject go = new GameObject("audio");
            go.transform.position = pos;
            AudioSource audio = go.AddComponent<AudioSource>();
            audio.clip = clip;
            audio.volume = vol;
            audio.pitch = pitch;
            audio.spatialBlend = 0;
            audio.outputAudioMixerGroup = Mixer.FindMatchingGroups("SFX")[0];
            audio.Play();
            Object.Destroy(go, clip.length + 10);
        }

        /// <summary>
        /// Play 3D sound.
        /// </summary>
        public static void PlaySound(Vector3 pos, AudioClip clip, float vol, float pitch, float minDist, float maxDist)
        {
            GameObject go = new GameObject("audio");
            go.transform.position = pos;
            AudioSource audio = go.AddComponent<AudioSource>();
            audio.clip = clip;
            audio.volume = vol;
            audio.pitch = pitch;
            audio.spatialBlend = 1;
            audio.spread = 90;
            audio.dopplerLevel = 0;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.minDistance = minDist;
            audio.maxDistance = maxDist;
            audio.outputAudioMixerGroup = Mixer.FindMatchingGroups("SFX")[0];
            audio.Play();
            Object.Destroy(go, clip.length + 10);
        }

        /// <summary>
        /// Get path total distance
        /// </summary>
        /// <param name="path">Input path as List</param>
        /// <returns>Distance as float</returns>
        public static float GetPathDistance(List<Vector3> path)
        {
            float result = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                result += Vector3.Distance(path[i], path[i + 1]);
            }

            return result;
        }

        /// <summary>
        /// Get points weight
        /// </summary>
        /// <param name="path">Input path as List</param>
        /// <param name="dist">Input path total distance</param>
        /// <returns>float array with points weights</returns>
        public static float[] GetPointsWeight(List<Vector3> path, float dist)
        {
            if (path.Count <= 0)
                return null;

            float[] result = new float[path.Count];
            float wholeDist = 0;
            result[0] = 0;

            for (int i = 1; i < result.Length; i++)
            {
                float lineDist = Vector3.Distance(path[i - 1], path[i]);
                wholeDist += lineDist;
                result[i] = wholeDist / dist;
            }

            return result;
        }

        /// <summary>
        /// Get point on path
        /// </summary>
        /// <param name="path">Input path as List</param>
        /// <param name="pointsWeights">Path points weights. Calculate through Utils.GetPointsWeight()</param>
        /// <param name="point">Point pos on path from 0 to 1</param>
        /// <returns>Vector3 pos</returns>
        public static Vector3 GetPathPoint(List<Vector3> path, float point, out Vector3 dir)
        {
            float[] pointsWeights = GetPointsWeight(path, GetPathDistance(path));
            point = Mathf.Clamp01(point);

            int rPoint = 0;
            for (int i = 1; i < pointsWeights.Length; i++)
            {
                if (point <= pointsWeights[i])
                {
                    rPoint = i;
                    break;
                }
            }
            int lPoint = rPoint - 1;

            dir = (path[rPoint] - path[lPoint]).normalized;
            return Vector3.Lerp(path[lPoint], path[rPoint], (point - pointsWeights[lPoint]) / (pointsWeights[rPoint] - pointsWeights[lPoint]));
        }

        /// <summary>
        /// Get point on path
        /// </summary>
        /// <returns>Vector3 pos</returns>
        public static Vector3 GetPathPoint(List<Vector3> path, float[] pointsWeights, float point, out Vector3 dir)
        {
            point = Mathf.Clamp01(point);

            int rPoint = 0;
            for (int i = 1; i < pointsWeights.Length; i++)
            {
                if (point <= pointsWeights[i])
                {
                    rPoint = i;
                    break;
                }
            }
            int lPoint = rPoint - 1;

            dir = (path[rPoint] - path[lPoint]).normalized;
            return Vector3.Lerp(path[lPoint], path[rPoint], (point - pointsWeights[lPoint]) / (pointsWeights[rPoint] - pointsWeights[lPoint]));
        }

        /// <summary>
        /// Returns the current scene bounds.
        /// Don't use it every frame!
        /// </summary>
        public static Bounds GetSceneBounds()
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer r in GameObject.FindObjectsOfType(typeof(Renderer)))
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }
    }
}