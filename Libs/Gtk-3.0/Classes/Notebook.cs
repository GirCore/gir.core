using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtk
{
    public partial class Notebook
    {
        public Notebook() { }

        public Widget this[string label]
        {
            set => AppendPage(value, Label.New(label));
        }
    }
}
