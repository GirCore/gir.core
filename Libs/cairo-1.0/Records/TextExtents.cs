using System.Runtime.InteropServices;

namespace cairo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TextExtents
    {
        public double xBearing;
        public double yBearing;
        public double width;
        public double height;
        public double xAdvance;
        public double yAdvance;
    }
}
