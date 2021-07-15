using System;
using System.Runtime.InteropServices;
using System.Text;

namespace cairo
{
    public partial class Context
    {
        // IMPORTANT: We should follow cairo's guidelines on memory management
        // for language bindings. This is a quick attempt at implementing something
        // workable.

        [DllImport ("cairo-graphics", EntryPoint = "cairo_set_source_rgba")]
        private static extern void NativeSetSourceRgba (Native.Context.Handle cr, double red, double green, double blue, double alpha);

        public void SetSourceRgba(double red, double green, double blue, double alpha)
            => NativeSetSourceRgba(Handle, red, green, blue, alpha);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_show_text")]
        private static extern void NativeShowText (Native.Context.Handle cr, [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8);

        public void ShowText(string text)
            => NativeShowText(Handle, text);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_move_to")]
        private static extern void NativeMoveTo (Native.Context.Handle cr, double x, double y);

        public void MoveTo(double x, double y)
            => NativeMoveTo(Handle, x, y);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_text_extents")]
        private static extern void NativeTextExtents (Native.Context.Handle cr, [MarshalAs (UnmanagedType.LPUTF8Str)] string utf8, out TextExtents extents);

        public void TextExtents(string text, out TextExtents extents)
            => NativeTextExtents(Handle, text, out extents);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_font_extents")]
        private static extern void NativeFontExtents (Native.Context.Handle cr, out FontExtents extents);

        public void FontExtents(out FontExtents extents)
            => NativeFontExtents(Handle, out extents);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_fill")]
        private static extern void NativeFill (Native.Context.Handle cr);

        public void Fill()
            => NativeFill(Handle);

        [DllImport ("cairo-graphics", EntryPoint = "cairo_rectangle")]
        private static extern void NativeRectangle (Native.Context.Handle cr, double x, double y, double width, double height);

        public void Rectangle(double x, double y, double width, double height)
            => NativeRectangle(Handle, x, y, width, height);


        [DllImport("cairo-graphics", EntryPoint = "cairo_set_font_size")]
        private static extern void NativeSetFontSize (Native.Context.Handle cr, double size);

        public void SetFontSize(double size)
            => NativeSetFontSize(Handle, size);
    }
}
