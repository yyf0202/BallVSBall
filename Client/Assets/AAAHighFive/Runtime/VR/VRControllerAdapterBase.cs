using UnityEngine;

namespace HighFive
{
    public enum VRControllerButtons
    {
        Enter,
        Touch,
        Back,
        Home,
        Menu,
        Exit
    }

    public delegate void OnControllerConnected(VRControllerAdapterBase controller);
    public delegate void OnControllerDisconnected(VRControllerAdapterBase controller);
    public delegate void OnControllerCustomEvent(VRControllerAdapterBase controller, int ev, object data);

    public abstract class VRControllerAdapterBase : MonoBehaviour
    {
        [HideInInspector]
        public string controllerTag;
        public VRBasePointer pointer;
        public OnControllerConnected onControllerConnected;
        public OnControllerDisconnected onControllerDisconnected;
        public OnControllerCustomEvent onControllerCustomEvent;

        // Use this for initialization
        protected virtual void Start()
        {
            VRControllers.Instance.RegisterController(this);
        }

        public abstract bool IsUsingGaze();

        public abstract bool GetControllerActive();

        public abstract bool GetButtonDown(VRControllerButtons button);

        public abstract bool GetButton(VRControllerButtons button);

        public abstract bool GetButtonUp(VRControllerButtons button);

        public abstract Vector2 GetTouchPos();

        public abstract void SetAlpha(float alpha);

        public virtual Transform GetHeartTrans()
        {
            return this.transform;
        }
    }
} 