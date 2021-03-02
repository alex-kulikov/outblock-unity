using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Display for entity hearing distance
    /// </summary>
    public class AIHearingCircle : MonoBehaviour
    {

        [SerializeField]
        private AIEntity entity = null;
        [SerializeField]
        private AISensors sensor = null;
        [SerializeField]
        private LineRenderer lineRenderer = null;

        private Color startColor;
        private int resolution = 16;

        private void Start()
        {
            startColor = lineRenderer.startColor;

            float radius = 1;
            List<Vector3> up = new List<Vector3>();
            List<Vector3> down = new List<Vector3>();

            float step = 180f / (float)resolution;
            for (int i = 0; i <= resolution; i++)
            {
                float x = radius * Mathf.Cos(step * i * Mathf.Deg2Rad);
                float y = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2));
                up.Add(new Vector3(x, 0, y));
                down.Add(new Vector3(x, 0, -y));
            }
            down.Reverse();

            List<Vector3> points = new List<Vector3>();
            points.AddRange(up);
            points.AddRange(down);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());

            InvokeRepeating("UpdateColor", 0.25f, 0.25f);
        }

        private void Update()
        {
            lineRenderer.enabled = entity.enabled && AIManager.closestToPlayer == entity;
            transform.localScale = Vector3.one * sensor.HearingRadius;
        }

        private void UpdateColor()
        {
            if (Player.Instance == null)
                return;

            float dist = Mathf.Clamp01(Vector3.Distance(Player.Instance.transform.position, transform.position) / AIVisionCone.maxDist);
            Color col = startColor;
            col.a *= 1 - dist;
            lineRenderer.startColor = col;
            lineRenderer.endColor = col;
        }
    }
}