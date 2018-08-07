using UnityEngine;

namespace HighFive
{
    public delegate void OnHMDConnectedEvent();
    public delegate void OnHMDMountedEvent();
    public delegate void OnHMDUnmountedEvent();
    public delegate void OnVRFocusAcquiredEvent();
    public delegate void OnVRFocusLostEvent();

    public class VRHMD : Singleton<VRHMD>
    {
        public OnHMDConnectedEvent onHMDConnected;
        public OnHMDMountedEvent onHMDMounted;
        public OnHMDUnmountedEvent onHMDUnmounted;
        public OnVRFocusAcquiredEvent onVRFocusAcquired;
        public OnVRFocusLostEvent onVRFocusLost;

        VRHMDAdapterBase current = null;

        public void RegisterHMD(VRHMDAdapterBase hmd)
        {
            current = hmd;

            current.onHMDConnected = InternalOnHMDConnected;
            current.onHMDMounted = InternalOnHMDMounted;
            current.onHMDUnmounted = InternalOnHMDUnmounted;
            current.onVRFocusAcquired = InternalOnVRFocusAcquired;
            current.onVRFocusLost = InternalOnVRFocusLost;
        }

        public VRHMDAdapterBase GetCurrentHMD()
        {
            return current;
        }

        public Quaternion GetOrientation()
        {
            if (current != null)
            {
                return current.GetOrientation();
            }
            else
            {
                return Quaternion.identity;
            }
        }

        public Vector3 GetPosition()
        {
            if (current != null)
            {
                return current.GetPosition();
            }
            else
            {
                return Vector3.zero;
            }
        }

        public void Recenter()
        {
            if (current != null)
            {
                current.Recenter();
            }
        }

        void InternalOnHMDConnected()
        {
            if (onHMDConnected != null)
            {
                onHMDConnected.Invoke();
            }
        }

        void InternalOnHMDMounted()
        {
            if (onHMDMounted != null)
            {
                onHMDMounted.Invoke();
            }
        }

        void InternalOnHMDUnmounted()
        {
            if (onHMDUnmounted != null)
            {
                onHMDUnmounted.Invoke();
            }
        }

        void InternalOnVRFocusAcquired()
        {
            if (onVRFocusAcquired != null)
            {
                onVRFocusAcquired.Invoke();
            }
        }

        void InternalOnVRFocusLost()
        {
            if (onVRFocusLost != null)
            {
                onVRFocusLost.Invoke();
            }
        }

    }
}