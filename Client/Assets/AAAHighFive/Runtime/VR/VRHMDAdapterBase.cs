using UnityEngine;

namespace HighFive
{
    public abstract class VRHMDAdapterBase : MonoBehaviour
    {
        [HideInInspector]
        public string HMDTag;
        public OnHMDConnectedEvent onHMDConnected;
        public OnHMDMountedEvent onHMDMounted;
        public OnHMDUnmountedEvent onHMDUnmounted;
        public OnVRFocusAcquiredEvent onVRFocusAcquired;
        public OnVRFocusLostEvent onVRFocusLost;

        protected virtual void Start()
        {
            VRHMD.Instance.RegisterHMD(this);

        }

        public abstract Quaternion GetOrientation();

        public abstract Vector3 GetPosition();

        public abstract void Recenter();
    }
}