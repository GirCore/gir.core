using GObject;

namespace Gtk
{
    public partial class Window
    {
        /// <summary>
        /// Creates a new instance of <see cref="Window"/>.
        /// </summary>
        /// <param name="title">The window title.</param>
        public Window(string title)
            : this(ConstructArgument.With("title", title))
        {
        }

        public void SetInteractiveDebugging(bool enable)
        {
            Native.Window.Instance.Methods.SetInteractiveDebugging(enable);
        }
    }
}
