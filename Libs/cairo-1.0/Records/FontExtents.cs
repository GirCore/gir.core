using System.Runtime.InteropServices;

namespace cairo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FontExtents
    {
        public double Ascent;
        public double Descent;
        public double Height;
        public double MaxXAdvance;
        public double MaxYAdvance;
    }
}
