using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace HighFive
{
    /// This script extends the standard Unity EventSystem events with VR head pointer specific events.
    public static class VRExecuteEventsExtension
    {
        private static readonly ExecuteEvents.EventFunction<IVRHeadPointerHoverHandler> s_HoverHandler = Execute;

        private static void Execute(IVRHeadPointerHoverHandler handler, BaseEventData eventData)
        {
            handler.OnHeadPointerHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IVRHeadPointerHoverHandler> pointerHoverHandler
        {
            get { return s_HoverHandler; }
        }

        private static readonly ObjectPool<List<IEventSystemHandler>> s_HandlerListPool = new ObjectPool<List<IEventSystemHandler>>(null, l => l.Clear());

        /// <summary>
        /// Get the specified object's event event.
        /// </summary>
        private static IEventSystemHandler GetEvent<T>(GameObject go, IList<IEventSystemHandler> results) where T : IEventSystemHandler
        {
            // Debug.LogWarning("GetEventList<" + typeof(T).Name + ">");
            if (results == null)
            {
                Debug.Log("Results array is null");
                return null;
            }

            if (go == null || !go.activeInHierarchy)
                return null;

            Component c = null;
            var components = ListPool<Component>.Get();
            go.GetComponents(components);
            for (var i = 0; i < components.Count; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;

                c = components[i];
                break;
            }

            ListPool<Component>.Release(components);
            return c == null ? default(T) : c as IEventSystemHandler;
        }

        public static IEventSystemHandler GetEvent<T>(GameObject go) where T : IEventSystemHandler
        {
            var internalHandlers = s_HandlerListPool.Get();
            IEventSystemHandler handler = GetEvent<T>(go, internalHandlers);
            s_HandlerListPool.Release(internalHandlers);
            return handler;
        }

        private static bool ShouldSendToComponent<T>(Component component) where T : IEventSystemHandler
        {
            var valid = component is T;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }
    }
}