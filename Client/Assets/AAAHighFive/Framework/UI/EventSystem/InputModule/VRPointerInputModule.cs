using UnityEngine;
using UnityEngine.EventSystems;


namespace HighFive
{
    public class VRPointerInputModule : BaseInputModule
    {
        public static VRPointerInputModule instance;

        /// Determines whether pointer input is active in VR Mode only (`true`), or all of the
        /// time (`false`).  Set to false if you plan to use direct screen taps or other
        /// input when not in VR Mode.
        [Tooltip("Whether pointer input is active in VR Mode only (true), or all the time (false).")]
        public bool vrModeOnly = false;

        [SerializeField]
        protected LayerMask headPointerEventMask = -1;

        private bool m_bLongPressBegin = false;
        //long press begin time, ms 
        private float m_longPressThreshold = 0.4f;
        //long press finish time, ms 
        private float m_longPressFinishThreshold = 1f;

        private PointerEventData pointerData;
        private Vector2 lastPose;
        private Vector2 lastScroll;
        private bool eligibleForScroll = false;

        // Active state
        private bool isActive = false;

        /// Time in seconds between the pointer down and up events sent by a trigger.
        /// Allows time for the UI elements to make their state transitions.
        private const float CLICK_TIME = 0.1f;
        // Based on default time for a button to animate to Pressed.

        /// Multiplier for calculating the scroll delta to that the scroll delta is
        /// within the order of magnitude that the UI system expects.
        private const float SCROLL_DELTA_MULTIPLIER = 50.0f;

        /// The IGvrPointer which will be responding to pointer events.
        private IVRPointer pointer
        {
            get
            {
                return VRPointerManager.Pointer;
            }
        }

        VRTouchInputModule touchInputModule;

        protected override void Awake()
        {
            instance = this;
            touchInputModule = this.GetComponent<VRTouchInputModule>();
        }

        /// @cond
        public override bool ShouldActivateModule()
        {
            bool isVrModeEnabled = !vrModeOnly;
            /*
            #if UNITY_EDITOR
            isVrModeEnabled |= VRSettings.enabled;
            #else
            isVrModeEnabled |= GvrViewer.Instance.VRModeEnabled;
            #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            */

            bool activeState = /*base.ShouldActivateModule() && */ isVrModeEnabled;

            if (activeState != isActive)
            {
                isActive = activeState;

                // Activate pointer
                if (pointer != null)
                {
                    if (isActive)
                    {
                        pointer.OnInputModuleEnabled();
                    }
                }
            }

            return activeState;
        }

        /// @endcond

        public override void DeactivateModule()
        {
            DisablePointer();
            base.DeactivateModule();
            if (pointerData != null)
            {
                HandlePendingClick();
                HandlePointerExitAndEnter(pointerData, null);
                pointerData = null;
            }
            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return pointerData != null && pointerData.pointerEnter != null;
        }

        public override void Process()
        {
            if (pointer == null)
            {
                //Debug.LogWarning("VRPointerInputModule requires VRPointerManager.Pointer to be set.");
                return;
            }

            // 2D Direct Touch Input Exceptions
            if (touchInputModule != null)
            {
                touchInputModule.Process();
                if (eventSystem.currentInputModule == touchInputModule && eventSystem.currentSelectedGameObject != null)
                {
                    return;
                }
            }

            if (!enabled) return;

            // Save the previous Game Object
            GameObject previousObject = GetCurrentGameObject();

            CastRay();
            UpdateCurrentObject(previousObject);
            UpdatePointer(previousObject);

            // True during the frame that the trigger has been pressed.
            bool triggerDown = false;//Input.GetMouseButtonDown(0);
                                     // True if the trigger is held down.
            bool triggering = false;//Input.GetMouseButton(0);

            // Handle Input using VRControllers Interface
            triggerDown |= VRControllers.Instance.GetButtonDown(VRControllerButtons.Enter);
            triggering |= VRControllers.Instance.GetButton(VRControllerButtons.Enter);

            //if (triggerDown | triggering)
            //{
            //    Debug.Log("triggering is " + triggering + " ,triggering is " + triggering);
            //}

            /*
            #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            triggerDown |= GvrController.ClickButtonDown;
            triggering |= GvrController.ClickButton;
            #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            */

            bool handlePendingClickRequired = !triggering;

            // Handle input
            if (!triggerDown && triggering)
            {
                HandleDrag();
            }
            else if (Time.unscaledTime - pointerData.clickTime < CLICK_TIME)
            {

                // Delay new events until clickTime has passed.
            }
            else if (triggerDown && !pointerData.eligibleForClick)
            {
                // New trigger action.
                HandleTriggerDown();
            }
            else if (handlePendingClickRequired)
            {
                // Check if there is a pending click to handle.
                HandlePendingClick();
            }

            HandleScroll();
        }


        System.Collections.Generic.List<RaycastResult> m_RaycastResultCache4headPointer = new System.Collections.Generic.List<RaycastResult>();





        protected void ComputeRayAndDistance(PointerEventData eventData, out Ray ray, out float distanceToClipPlane)
        {
            ray = Camera.main.ScreenPointToRay(eventData.position);
            // compensate far plane distance - see MouseEvents.cs
            float projectionDirection = ray.direction.z;
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity : Mathf.Abs((Camera.main.farClipPlane - Camera.main.nearClipPlane) / projectionDirection);
        }

        /// @endcond

        private void CastRay()
        {
            Vector2 currentPose = NormalizedCartesianToSpherical(pointer.GetPointerTransform().forward);

            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
                lastPose = currentPose;
            }

            // Store the previous raycast result.
            RaycastResult previousRaycastResult = pointerData.pointerCurrentRaycast;

            // The initial cast must use the enter radius.
            if (pointer != null)
            {
                pointer.ShouldUseExitRadiusForRaycast = false;
            }

            // Cast a ray into the scene
            pointerData.Reset();
            // Set the position to the center of the camera.
            // This is only necessary if using the built-in Unity raycasters.
            pointerData.position = GetViewportCenter();
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);

            // If we were already pointing at an object we must check that object against the exit radius
            // to make sure we are no longer pointing at it to prevent flicker.
            if (previousRaycastResult.gameObject != null && raycastResult.gameObject != previousRaycastResult.gameObject)
            {
                if (pointer != null)
                {
                    pointer.ShouldUseExitRadiusForRaycast = true;
                }
                m_RaycastResultCache.Clear();
                eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
                RaycastResult firstResult = FindFirstRaycast(m_RaycastResultCache);
                if (firstResult.gameObject == previousRaycastResult.gameObject)
                {
                    raycastResult = firstResult;
                }
            }

            if (raycastResult.gameObject != null && raycastResult.worldPosition == Vector3.zero)
            {
                raycastResult.worldPosition = GetIntersectionPosition(pointerData.enterEventCamera, raycastResult);
            }

            pointerData.pointerCurrentRaycast = raycastResult;

            // Find the real screen position associated with the raycast
            // Based on the results of the hit and the state of the pointerData.
            if (raycastResult.gameObject != null)
            {
                pointerData.position = raycastResult.screenPosition;
            }
            else
            {
                Transform pointerTransform = pointer.GetPointerTransform();
                float maxPointerDistance = pointer.GetMaxPointerDistance();
                Vector3 pointerPos = pointerTransform.position + (pointerTransform.forward * maxPointerDistance);
                if (pointerData.pressEventCamera != null)
                {
                    pointerData.position = pointerData.pressEventCamera.WorldToScreenPoint(pointerPos);
                }
                else if (Camera.main != null)
                {
                    pointerData.position = Camera.main.WorldToScreenPoint(pointerPos);
                }
            }

            m_RaycastResultCache.Clear();
            pointerData.delta = currentPose - lastPose;
            lastPose = currentPose;
        }

        private void UpdateCurrentObject(GameObject previousObject)
        {
            // Send enter events and update the highlight.
            GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
            HandlePointerExitAndEnter(pointerData, previousObject);

            // Update the current selection, or clear if it is no longer the current object.
            var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(currentObject);
            if (selected == eventSystem.currentSelectedGameObject)
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
                    ExecuteEvents.updateSelectedHandler);
            }
            else
            {
                eventSystem.SetSelectedGameObject(null, pointerData);
            }
        }

        private void UpdatePointer(GameObject previousObject)
        {
            if (pointer == null)
            {
                return;
            }

            GameObject currentObject = GetCurrentGameObject(); // Get the pointer target
            Vector3 intersectionPosition = pointerData.pointerCurrentRaycast.worldPosition;
            bool isInteractive = pointerData.pointerPress != null ||
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject) != null ||
                ExecuteEvents.GetEventHandler<IDragHandler>(currentObject) != null;

            if (currentObject == previousObject)
            {
                if (currentObject != null)
                {
                    pointer.OnPointerHover(currentObject, intersectionPosition, GetLastRay(), isInteractive, pointerData);
                }
            }
            else
            {
                if (previousObject != null)
                {
                    pointer.OnPointerExit(previousObject);
                }

                if (currentObject != null)
                {
                    pointer.OnPointerEnter(currentObject, intersectionPosition, GetLastRay(), isInteractive, pointerData);
                }
            }

            // Rare cases: currentObject == previousObject and they are all null,
            // Call pointer.OnPointerExit
            if (currentObject == null && previousObject == null)
            {
                pointer.OnPointerExit(null);
            }
        }
        public bool BEnableLongPress = true;
        private void HandleDrag()
        {
            bool moving = pointerData.IsPointerMoving();

            if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
            {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                    ExecuteEvents.beginDragHandler);
                pointerData.dragging = true;
            }

            // Drag notification
            if (pointerData.dragging && moving && pointerData.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerData.pointerPress != pointerData.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

                    pointerData.eligibleForClick = false;
                    pointerData.pointerPress = null;
                    pointerData.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
            }
        }

        private void HandlePendingClick()
        {
            if (!pointerData.eligibleForClick && !pointerData.dragging)
            {
                return;
            }

            if (pointer != null)
            {
                pointer.OnPointerClickUp();
            }

            if (!m_bLongPressBegin)
            {
                var go = pointerData.pointerCurrentRaycast.gameObject;
                // Send pointer up and click events.
                ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

                if (pointerData.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

                    if (pointerData.pointerPress == null && pointerData.pointerCurrentRaycast.gameObject == null)
                    {
                        //RawInputModule.Instance.OnPointerClickNothing();
                    }
                }
                else if (pointerData.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
                    ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
                }
            }
            else
                ;
            //RawInputModule.Instance.OnPointerLongPressFinish();

            // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.clickTime = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;
            m_bLongPressBegin = false;
        }

        private void HandleTriggerDown()
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer down event.
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

            // Save the drag handler as well
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
            if (pointerData.pointerDrag != null)
            {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
            }

            // Save the pending click state.
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;


            if (pointer != null)
            {
                pointer.OnPointerClickDown();
            }
        }

        ScrollEventData scrollEventData;
        private void HandleScroll()
        {
            bool touchDown = false;
            bool touching = false;
            Vector2 currentScroll = Vector2.zero;

            int cidx = -1;
            int cidxa = -1;

            touchDown |= VRControllers.Instance.GetButtonDown(VRControllerButtons.Touch, out cidx);
            touching |= VRControllers.Instance.GetButton(VRControllerButtons.Touch, out cidxa);
            if (cidx > -1)
            {
                currentScroll = VRControllers.Instance.GetTouchPos(cidx);
            }
            else if (cidxa > -1)
            {
                currentScroll = VRControllers.Instance.GetTouchPos(cidxa);
            }

            /*
            #if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            touchDown |= GvrController.TouchDown;
            touching |= GvrController.IsTouching;
            currentScroll = GvrController.TouchPos;
            #endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
            */

            if (touchDown && !eligibleForScroll)
            {
                lastScroll = currentScroll;
                eligibleForScroll = true;
            }
            else if (touching && eligibleForScroll)
            {
                //Debug.Log("currentScroll is " + currentScroll + " ， lastScroll is " + lastScroll);
                pointerData.scrollDelta = (currentScroll - lastScroll) * SCROLL_DELTA_MULTIPLIER;

                GameObject currentGameObject = GetCurrentGameObject();
                if (currentGameObject != null && !Mathf.Approximately(pointerData.scrollDelta.sqrMagnitude, 0.0f))
                {
                    GameObject scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(currentGameObject);
                    ExecuteEvents.ExecuteHierarchy(scrollHandler, pointerData, ExecuteEvents.scrollHandler);
                }

                if (scrollEventData == null)
                    scrollEventData = new ScrollEventData(eventSystem);

                if (currentGameObject != null && Mathf.Abs((currentScroll - lastScroll).x) >= 1 && Mathf.Abs(currentScroll.x) > float.Epsilon)
                {
                    scrollEventData.Direction = (currentScroll - lastScroll).x >= 1 ? ScrollEventData.ScrollDirection.Right : ScrollEventData.ScrollDirection.Left;

                    GameObject scrollHandler = ExecuteEvents.GetEventHandler<IScrollToDirectHandler>(currentGameObject);
                    scrollEventData.selectedObject = scrollHandler;
                    ExecuteEvents.Execute(scrollHandler, scrollEventData, VRExecuteEvents.scrollToDirectHandler);
                }

                lastScroll = currentScroll;
            }
            else if (eligibleForScroll)
            {
                eligibleForScroll = false;
                pointerData.scrollDelta = Vector2.zero;
            }
        }

        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
        {
            cartCoords.Normalize();
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            float outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }

        private GameObject GetCurrentGameObject()
        {
            if (pointerData != null)
            {
                return pointerData.pointerCurrentRaycast.gameObject;
            }

            return null;
        }

        private Ray GetLastRay()
        {
            if (pointerData != null)
            {
                VRBasePointerRaycaster raycaster = pointerData.pointerCurrentRaycast.module as VRBasePointerRaycaster;
                if (raycaster != null)
                {
                    return raycaster.GetLastRay();
                }
                else if (pointerData.enterEventCamera != null)
                {
                    Camera cam = pointerData.enterEventCamera;
                    return new Ray(cam.transform.position, cam.transform.forward);
                }
            }

            return new Ray();
        }

        private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
        {
            // Check for camera
            if (cam == null)
            {
                return Vector3.zero;
            }

            float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
            Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
            return intersectionPosition;
        }

        private void DisablePointer()
        {
            if (pointer == null)
            {
                return;
            }

            GameObject currentGameObject = GetCurrentGameObject();
            if (currentGameObject)
            {
                pointer.OnPointerExit(currentGameObject);
            }

            pointer.OnInputModuleDisabled();
        }

        private Vector2 GetViewportCenter()
        {
            int viewportWidth = Screen.width;
            int viewportHeight = Screen.height;
            //#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID
            // GVR native integration is supported
#if !UNITY_EDITOR
        if (UnityEngine.XR.XRSettings.enabled)
        {
            viewportWidth = UnityEngine.XR.XRSettings.eyeTextureWidth;
            viewportHeight = UnityEngine.XR.XRSettings.eyeTextureHeight;
        }
#endif
            //#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && UNITY_ANDROID

            return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
        }
    }
}