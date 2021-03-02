using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This struct represents the point on the trajectory.
    /// </summary>
    public struct TrajectoryPoint
    {

        /// <summary>
        /// World position of the point.
        /// </summary>
        public Vector3 point { get; set; }

        /// <summary>
        /// Is trajectory touched the surface at this point?
        /// </summary>
        public bool bounce { get; private set; }

        /// <summary>
        /// Time of fligth at this point.
        /// </summary>
        public float time { get; private set; }

        /// <summary>
        /// Creates new TrajectoryPoint object.
        /// </summary>
        /// <param name="point">World position of the point.</param>
        /// <param name="bounce">Is trajectory touched the surface at this point?</param>
        /// <param name="time">Time of fligth at this point.</param>
        public TrajectoryPoint(Vector3 point, bool bounce, float time) : this()
        {
            this.point = point;
            this.bounce = bounce;
            this.time = time;
        }

    }
}