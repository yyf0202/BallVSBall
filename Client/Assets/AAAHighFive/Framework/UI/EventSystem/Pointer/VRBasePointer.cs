using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace HighFive
{
    public abstract class VRBasePointer : MonoBehaviour, IVRPointer
    {

        protected virtual void Start()
        {
            VRPointerManager.Instance.RegisterPointer(this);
        }

        public bool ShouldUseExitRadiusForRaycast
        {
            get;
            set;
        }

        /// Declare methods from IGvrPointer
        public abstract void OnInputModuleEnabled();

        public abstract void OnInputModuleDisabled();

        public virtual void OnPointerEnter(GameObject targetObject, Vector3 intersectionPosition,
                                           Ray intersectionRay, bool isInteractive, PointerEventData eventData)
        {

            if (VRPointerManager.Instance.globalOnPointerEnter != null)
                VRPointerManager.Instance.globalOnPointerEnter.Invoke();
        }

        public virtual void OnPointerHover(GameObject targetObject, Vector3 intersectionPosition,
                                           Ray intersectionRay, bool isInteractive, PointerEventData eventData)
        {

            if (VRPointerManager.Instance.globalOnPointerHover != null)
                VRPointerManager.Instance.globalOnPointerHover.Invoke();
        }

        public virtual void OnPointerExit(GameObject targetObject)
        {
            if (VRPointerManager.Instance.globalOnPointerExit != null)
                VRPointerManager.Instance.globalOnPointerExit.Invoke();
        }

        public virtual void OnPointerClickDown()
        {
            if (VRPointerManager.Instance.globalOnPointerClickDown != null)
                VRPointerManager.Instance.globalOnPointerClickDown.Invoke();
        }

        public virtual void OnPointerClickUp()
        {
            if (VRPointerManager.Instance.globalOnPointerClickUp != null)
                VRPointerManager.Instance.globalOnPointerClickUp.Invoke();
        }

        public abstract float GetMaxPointerDistance();

        public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);

        public virtual Transform GetPointerTransform()
        {
            return transform;
        }

        public abstract void SetAlpha(float alpha);

        public abstract GameObject GetCurrentObject();
    }
}