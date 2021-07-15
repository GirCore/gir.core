namespace Gtk
{
    public partial class IMContextSimple
    {
        public static IMContextSimple New()
            => new IMContextSimple(Native.IMContextSimple.Instance.Methods.New(), false);
    }
}
