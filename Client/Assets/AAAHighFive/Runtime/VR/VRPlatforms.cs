using System.Collections;
using UnityEngine;


namespace HighFive
{
    public class VRPlatforms : Singleton<VRPlatforms>
    {
        private VRPlatformAdapterBase current;

        public void RegisterPlatform(VRPlatformAdapterBase platform)
        {
            current = platform;
        }

        public void AppEvent(int ev, object data)
        {
            if (current != null)
            {
                current.PlatformAppEvent(ev, data);
            }
        }

        public void Init()
        {
            if (current != null)
            {
                current.PlatformInit();
            }
        }

        public void Quit()
        {
            if (current != null)
            {
                current.PlatformQuit();
            }
        }

        public void Menu()
        {
            if (current != null)
            {
                current.PlatformMenu();
            }
        }
    }
}