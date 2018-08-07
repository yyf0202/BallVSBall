namespace UnityEngine.EventSystems
{
    public interface IScrollToDirectHandler : IEventSystemHandler
    {
        void OnScrollToLeft(ScrollEventData eventData);
    }

    public static class VRExecuteEvents
    {
        private static readonly ExecuteEvents.EventFunction<IScrollToDirectHandler> s_ScrollToDirectHandler = Execute;

        private static void Execute(IScrollToDirectHandler handler, BaseEventData eventData)
        {
            handler.OnScrollToLeft(ExecuteEvents.ValidateEventData<ScrollEventData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IScrollToDirectHandler> scrollToDirectHandler
        {
            get { return s_ScrollToDirectHandler; }
        }
    }
}