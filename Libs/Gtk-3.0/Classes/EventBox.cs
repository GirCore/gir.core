namespace Gtk
{
    public partial class EventBox
    {
        public static EventBox New()
            => new EventBox(Native.EventBox.Instance.Methods.New(), false);
    }
}
