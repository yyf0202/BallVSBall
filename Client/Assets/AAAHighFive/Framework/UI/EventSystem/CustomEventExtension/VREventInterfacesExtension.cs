using UnityEngine.EventSystems;

namespace HighFive
{
    /// Interface to implement if you wish to receive OnHeadPointerHover callbacks.
    public interface IVRHeadPointerHoverHandler : IEventSystemHandler
    {

        /// Called when pointer is hovering over GameObject.
        void OnHeadPointerHover(PointerEventData eventData);
    }
}