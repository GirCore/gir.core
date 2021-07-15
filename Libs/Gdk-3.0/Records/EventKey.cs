using System;
using System.Runtime.InteropServices;

namespace Gdk
{
    public partial class EventKey : Event
    {
        // TODO: Proof-of-concept for accessing record fields. We'll want to generate this eventually
        
        public Gdk.EventType Type
        {
            get => Marshal.PtrToStructure<Native.EventKey.Struct>(Handle.DangerousGetHandle()).Type;
        }
        
        public uint Keyval
        {
            get => Marshal.PtrToStructure<Native.EventKey.Struct>(Handle.DangerousGetHandle()).Keyval;
        }
        
        public ushort HardwareKeycode
        {
            get => Marshal.PtrToStructure<Native.EventKey.Struct>(Handle.DangerousGetHandle()).HardwareKeycode;
        }
    }
}
