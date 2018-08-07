using System;
using System.Text;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    public class ScrollEventData : BaseEventData
    {
        public enum ScrollDirection
        {
            Unknow,
            Left,
            Right,
            Up,
            Down
        }

        public ScrollDirection Direction { get; set; }

        public ScrollEventData(EventSystem eventSystem) : base(eventSystem)
        {
            Direction = ScrollDirection.Unknow;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>selectedObject</b>: " + selectedObject.name);
            sb.AppendLine("<b>direction</b>: " + Direction);
            return sb.ToString();
        }
    }
}
