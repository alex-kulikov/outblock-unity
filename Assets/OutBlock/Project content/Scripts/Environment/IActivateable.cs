using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Possible teams.
    /// </summary>
    public enum Teams { Player, AI, Both };

    /// <summary>
    /// Interface for the SignalSource which are interact with the IActivateable/IDeactivatable
    /// </summary>
    public interface ISignalSource
    {
        /// <summary>
        /// Transform of the signal.
        /// </summary>
        Transform AssignedTransform { get; }

        /// <summary>
        /// Team of the signal.
        /// </summary>
        Teams Team { get; }
    }

    /// <summary>
    /// Interface for the objects that can be activated.
    /// </summary>
    public interface IActivatable
    {
        /// <summary>
        /// Which team can activate this?
        /// </summary>
        Teams TargetTeam { get; }

        /// <summary>
        /// Activate this object.
        /// </summary>
        void Activate(ISignalSource source);
    }

    /// <summary>
    /// Interface for the objects that can be deactivated.
    /// </summary>
    public interface IDeactivatable
    {
        /// <summary>
        /// Deactivate this object.
        /// </summary>
        void Deactivate(ISignalSource source);
    }
}