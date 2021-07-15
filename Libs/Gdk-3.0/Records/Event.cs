using System;
using System.Runtime.InteropServices;

namespace Gdk
{
    public partial class Event
    {
        public static Event __FactoryNew(IntPtr ptr)
        {
            var ev = Marshal.PtrToStructure<Native.EventAny.Struct>(ptr);
            switch (ev.Type)
            {
                case EventType.Expose:
                    return EventExpose.__FactoryNew(ptr);
                case EventType.VisibilityNotify:
                    return EventVisibility.__FactoryNew(ptr);
                case EventType.MotionNotify:
                    return EventMotion.__FactoryNew(ptr);
                case EventType.ButtonPress:
                case EventType.TwoButtonPress:
                case EventType.ThreeButtonPress:
                case EventType.ButtonRelease:
                    return EventButton.__FactoryNew(ptr);
                case EventType.TouchBegin:
                    return EventTouch.__FactoryNew(ptr);
                case EventType.Scroll:
                    return EventScroll.__FactoryNew(ptr);
                case EventType.KeyPress:
                case EventType.KeyRelease:
                    return EventKey.__FactoryNew(ptr);
                case EventType.EnterNotify:
                case EventType.LeaveNotify:
                    return EventCrossing.__FactoryNew(ptr);
                case EventType.FocusChange:
                    return EventFocus.__FactoryNew(ptr);
                case EventType.Configure:
                    return EventConfigure.__FactoryNew(ptr);
                case EventType.PropertyNotify:
                    return EventProperty.__FactoryNew(ptr);
                case EventType.SelectionClear:
                case EventType.SelectionNotify:
                case EventType.SelectionRequest:
                    return EventSelection.__FactoryNew(ptr);
                case EventType.OwnerChange:
                    return EventOwnerChange.__FactoryNew(ptr);
                case EventType.ProximityIn:
                case EventType.ProximityOut:
                    return EventProximity.__FactoryNew(ptr);
                case EventType.DragEnter:
                case EventType.DragLeave:
                case EventType.DragMotion:
                case EventType.DragStatus:
                case EventType.DropStart:
                case EventType.DropFinished:
                    return EventDND.__FactoryNew(ptr);
                case EventType.WindowState:
                    return EventWindowState.__FactoryNew(ptr);
                case EventType.Setting:
                    return EventSetting.__FactoryNew(ptr);
                case EventType.GrabBroken:
                    return EventGrabBroken.__FactoryNew(ptr);
                case EventType.TouchpadSwipe:
                    return EventTouchpadSwipe.__FactoryNew(ptr);
                case EventType.TouchpadPinch:
                    return EventTouchpadPinch.__FactoryNew(ptr);
                case EventType.PadButtonPress:
                case EventType.PadButtonRelease:
                    return EventPadButton.__FactoryNew(ptr);
                case EventType.PadRing:
                case EventType.PadStrip:
                    return EventPadAxis.__FactoryNew(ptr);
                case EventType.PadGroupMode:
                    return EventPadGroupMode.__FactoryNew(ptr);
                // Default/Not Implemented
                case EventType.Map:
                case EventType.Unmap:
                case EventType.Delete:
                case EventType.Destroy:
                default:
                    throw new NotImplementedException("Event Type not yet supported");
            }
        }
    }
}
