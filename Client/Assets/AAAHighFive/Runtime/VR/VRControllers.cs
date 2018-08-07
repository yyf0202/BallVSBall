using UnityEngine;
using System.Collections.Generic;

namespace HighFive
{
    public class VRControllers : Singleton<VRControllers>
    {
        private List<VRControllerAdapterBase> controllers = new List<VRControllerAdapterBase>();

        public OnControllerConnected onControllerConnected;
        public OnControllerDisconnected onControllerDisconnected;
        public OnControllerCustomEvent onControllerCustomEvent;

        void Awake()
        {
          
        }

        public void RegisterController(VRControllerAdapterBase controller)
        {
            if (!controllers.Contains(controller))
            {
                controllers.Add(controller);

                controller.onControllerConnected = InternalOnControllerConnected;
                controller.onControllerCustomEvent = InternalOnControllerCustomEvent;
                controller.onControllerDisconnected = InternalOnControllerDisconnected;
            }
        }

        public void RemoveController(VRControllerAdapterBase controller)
        {
            controllers.Remove(controller);
        }

        public int GetControllerCount()
        {
            return controllers.Count;
        }

        public bool IsUsingGaze()
        {
            bool r = false;
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].GetControllerActive())
                {
                    r = r || controllers[i].IsUsingGaze();
                }
            }
            return r;
        }

        public int GetActiveControllerCount()
        {
            int c = 0;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                {
                    c++;
                }
            }

            return c;
        }

        public int GetControllerIndex(VRControllerAdapterBase controller)
        {
            return controllers.IndexOf(controller);
        }

        public bool HasControllerTagActive(string controllerTag)
        {
            for (int i = 0; i < GetControllerCount(); i++)
            {
                if (controllers[i].GetControllerActive() && (controllers[i].controllerTag == controllerTag))
                {
                    return true;
                }
            }
            return false;
        }

        public VRControllerAdapterBase GetController(int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                return controllers[controllerIndex];
            }

            return null;
        }

        public bool GetButtonDown(VRControllerButtons button)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r |= controllers[i].GetButtonDown(button);

                if (r) break;
            }
            return r;
        }

        public bool GetButtonDown(VRControllerButtons button, int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                if (controllers[controllerIndex].GetControllerActive())
                {
                    return controllers[controllerIndex].GetButtonDown(button);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool GetButtonDown(VRControllerButtons button, out int controllerIndex)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r = controllers[i].GetButtonDown(button);

                if (r)
                {
                    controllerIndex = i;
                    return true;
                }
            }
            controllerIndex = -1;
            return r;
        }


        public bool GetButton(VRControllerButtons button)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r |= controllers[i].GetButton(button);
                if (r) break;
            }
            return r;
        }

        public bool GetButton(VRControllerButtons button, int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                return controllers[controllerIndex].GetControllerActive() && controllers[controllerIndex].GetButton(button);
            }
            else
            {
                return false;
            }
        }

        public bool GetButton(VRControllerButtons button, out int controllerIndex)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r = controllers[i].GetButton(button);
                if (r)
                {
                    controllerIndex = i;
                    return true;
                }
            }
            controllerIndex = -1;
            return r;
        }

        public bool GetButtonUp(VRControllerButtons button)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r |= controllers[i].GetButtonUp(button);
                if (r) break;
            }
            return r;
        }

        public bool GetButtonUp(VRControllerButtons button, int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                return controllers[controllerIndex].GetControllerActive() && controllers[controllerIndex].GetButtonUp(button);
            }
            else
            {
                return false;
            }
        }

        public bool GetButtonUp(VRControllerButtons button, out int controllerIndex)
        {
            bool r = false;
            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                if (controllers[i].GetControllerActive())
                    r = controllers[i].GetButtonUp(button);
                if (r)
                {
                    controllerIndex = i;
                    return true;
                }
            }
            controllerIndex = -1;
            return r;
        }

        public Vector2 GetTouchPos(int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count && controllers[controllerIndex].GetControllerActive())
            {
                return controllers[controllerIndex].GetTouchPos();
            }
            else
            {
                return Vector2.zero;
            }
        }

        public void HideController(int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                controllers[controllerIndex].SetAlpha(0f);
            }
        }

        public void ShowController(int controllerIndex)
        {
            if (controllerIndex >= 0 && controllerIndex < controllers.Count)
            {
                controllers[controllerIndex].SetAlpha(1f);
            }
        }

        public void HideAllControllers()
        {
            // songlingyi temp

            //for (int i = 0; i < controllers.Count; i++)
            //{
            //    controllers[i].SetAlpha(0f);
            //}
        }

        public void ShowAllControllers()
        {
            // songlingyi temp

            //for (int i = 0; i < controllers.Count; i++)
            //{
            //    controllers[i].SetAlpha(1f);
            //}
        }

        void InternalOnControllerConnected(VRControllerAdapterBase controller)
        {
            if (onControllerConnected != null) onControllerConnected.Invoke(controller);
        }

        void InternalOnControllerDisconnected(VRControllerAdapterBase controller)
        {
            if (onControllerDisconnected != null) onControllerDisconnected.Invoke(controller);
        }

        void InternalOnControllerCustomEvent(VRControllerAdapterBase controller, int ev, object data)
        {
            if (onControllerCustomEvent != null) onControllerCustomEvent.Invoke(controller, ev, data);
        }
    }
}