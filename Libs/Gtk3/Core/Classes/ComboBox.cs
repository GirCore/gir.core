using System;
using System.Reflection;
using GObject;

namespace Gtk
{
    public partial class ComboBox
    {
        internal ComboBox(string template, string obj, Assembly assembly) : base(template, obj, assembly)
        { }
    }
}