namespace Gtk
{
    public partial class Paned
    {
        public static Paned New(Orientation orientation)
            => new Paned(Native.Paned.Instance.Methods.New(orientation), false);
    }
}
