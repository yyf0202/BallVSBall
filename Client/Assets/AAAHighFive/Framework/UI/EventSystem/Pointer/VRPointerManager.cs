using UnityEngine.Events;
using System.Collections.Generic;


namespace HighFive
{
    /// VRPointerManager is a standard interface for
    /// controlling which IVRPointer is being used
    /// for user input affordance.
    public class VRPointerManager : Singleton<VRPointerManager>
    {
        public UnityAction globalOnPointerEnter;
        public UnityAction globalOnPointerExit;
        public UnityAction globalOnPointerHover;
        public UnityAction globalOnPointerClickDown;
        public UnityAction globalOnPointerClickUp;

        private List<VRBasePointer> pointers = new List<VRBasePointer>();

        void Awake()
        {

        }

        /// Change the IVRPointer that is currently being used.
        public static VRBasePointer Pointer
        {
            get
            {
                return Instance == null ? null : Instance.pointer;
            }
            set
            {
                if (Instance == null || Instance.pointer == value)
                {
                    return;
                }

                Instance.pointer = value;
            }
        }

        /// VRBasePointer calls this when it is created.
        /// If a pointer hasn't already been assigned, it
        /// will assign the newly created one by default.
        ///
        /// This simplifies the common case of having only one
        /// IVRPointer so is can be automatically hooked up
        /// to the manager.  If multiple VRBasePointer are in
        /// the scene, the app has to take responsibility for
        /// setting which one is active.
        public void RegisterPointer(VRBasePointer pointer)
        {
            if (Instance == null)
                return;

            if (!pointers.Contains(pointer))
            {
                pointers.Add(pointer);
            }
        }

        public void RemovePointer(VRBasePointer pointer)
        {
            if (pointer == null)
                return;

            pointers.Remove(pointer);

            if (VRPointerManager.Pointer == pointer)
                VRPointerManager.Pointer = pointers.Count == 0 ? null : pointers[pointers.Count - 1];

            if (VRPointerManager.Pointer != null && VRPointerManager.Pointer.gameObject.activeSelf)
                VRPointerManager.Pointer.gameObject.SetActive(true);
        }

        private VRBasePointer pointer;

        public int GetPointerCount()
        {
            return pointers.Count;
        }

        public VRBasePointer GetPointer(int index)
        {
            if (index >= 0 && index < pointers.Count)
            {
                return pointers[index];
            }

            return null;
        }
    }
}