using System;
using GObject.Native;

namespace Gtk
{
    public partial class TreeSelection
    {
        public void GetSelected(out TreeModel model, out TreeIter iter)
        {
            var iterHandle = new Native.TreeIter.Handle(IntPtr.Zero);
            Native.TreeSelection.Instance.Methods.GetSelected(Handle, out var modelPtr, iterHandle);

            model = ObjectWrapper.WrapHandle<TreeModel>(modelPtr, false);
            iter = TreeIter.__FactoryNew(iterHandle);
        }
    }
}
