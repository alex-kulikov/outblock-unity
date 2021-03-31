using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Denver.Metrics
{
    public static partial class Utils
    {

        /// <summary>
        /// Converts float time value to the correct time string - minutes:seconds
        /// </summary>
        public static string FloatToTime(float value)
        {
            int minutes = (int)(value / 60);
            int seconds = (int)(value - minutes * 60);
            return string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00"));
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
        /// <param name="point">Point pos on path from 0 to 1</param>
        /// <returns>Vector3 pos</returns>
        public static Vector3 GetPathPoint(List<Vector3> path, float point, out int lastPoint)
        {
            point = Mathf.Clamp01(point);
            float[] pointsWeights = GetPointsWeight(path, GetPathDistance(path));

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

            lastPoint = lPoint;
            return Vector3.Lerp(path[lPoint], path[rPoint], (point - pointsWeights[lPoint]) / (pointsWeights[rPoint] - pointsWeights[lPoint]));
        }

        /// <summary>
        /// Draws the gizmo.
        /// </summary>
        public static void DrawGizmo(Vector3 pos, Color gizmoColor, GizmoTypes gizmoType, float gizmoSize)
        {
            Gizmos.color = gizmoColor;
            switch (gizmoType)
            {
                case GizmoTypes.Cube:
                    Gizmos.DrawCube(pos, Vector3.one * gizmoSize);
                    break;

                case GizmoTypes.Sphere:
                    Gizmos.DrawSphere(pos, gizmoSize / 2);
                    break;

                case GizmoTypes.WireCube:
                    Gizmos.DrawWireCube(pos, Vector3.one * gizmoSize);
                    break;

                case GizmoTypes.WireSphere:
                    Gizmos.DrawWireSphere(pos, gizmoSize / 2);
                    break;
            }
        }

        private static void DrawPathSector(ref List<Vector3> points, int index)
        {
            Gizmos.DrawWireCube(points[index], Vector3.one * 0.1f);

            if (index < points.Count - 1)
                Gizmos.DrawLine(points[index], points[index + 1]);
        }

        /// <summary>
        /// Draws path with specified color.
        /// </summary>
        public static void DrawPath(List<Vector3> points, Color color)
        {
            Gizmos.color = color;

            for (int i = 0; i < points.Count; i++)
            {
                DrawPathSector(ref points, i);
            }
        }

        /// <summary>
        /// Draws path with specified color and camera check.
        /// </summary>
        public static void DrawPathCheck(List<Vector3> points, Color color, Plane[] planes)
        {
            Gizmos.color = color;
            Bounds testBound = new Bounds(Vector3.zero, Vector3.one * 0.05f);

            for (int i = 0; i < points.Count; i++)
            {
                testBound.center = points[i];

                if (!GeometryUtility.TestPlanesAABB(planes, testBound))
                    continue;

                DrawPathSector(ref points, i);
            }
        }

        /// <summary>
        /// Converts float to string with correct format.
        /// </summary>
        public static string WriteFloat(float value)
        {
            return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts string to float with correct format.
        /// </summary>
        public static float ReadFloat(string value)
        {
            return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get the directory where files is going to be stored.
        /// </summary>
        /// <returns>Directory path.</returns>
        public static string GetDirectoryPath()
        {
#if UNITY_EDITOR
            return Path.GetDirectoryName(SceneManager.GetActiveScene().path) + Path.DirectorySeparatorChar + SceneManager.GetActiveScene().name;
#else
            return Application.dataPath + Path.DirectorySeparatorChar + SceneManager.GetActiveScene().name;
#endif
        }

        /// <summary>
        /// Get the signals file path.
        /// </summary>
        /// <returns>Returns path to the file.</returns>
        public static string GetSignalsFilePath()
        {
            return GetDirectoryPath() + "_signals.txt";
        }

        /// <summary>
        /// Get the paths file path.
        /// </summary>
        /// <returns>Returns path to the file.</returns>
        public static string GetPathsFilePath()
        {
            return GetDirectoryPath() + "_paths.txt";
        }
    }
}
