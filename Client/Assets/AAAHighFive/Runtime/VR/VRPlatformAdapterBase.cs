using UnityEngine;

namespace HighFive
{
    public abstract class VRPlatformAdapterBase : MonoBehaviour
    {
        [HideInInspector]
        public string platformTag;

        protected virtual void Start()
        {
            VRPlatforms.Instance.RegisterPlatform(this);
        }

        public abstract void PlatformQuit();

        public abstract void PlatformInit();

        public abstract void PlatformMenu();

        public abstract void PlatformAppEvent(int ev, object data);

    }
}