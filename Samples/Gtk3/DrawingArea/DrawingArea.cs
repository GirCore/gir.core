using System;
using Gtk;

Functions.Init();

var window = new Window("DrawingArea Demo");
var drawingArea = DrawingArea.New();
window.Child = drawingArea;

drawingArea.OnDraw += (d, a) =>
{
    cairo.Context cr = a.Cr;
    cr.SetSourceRgba(0.1, 0.1, 0.1, 1.0);
    cr.MoveTo(20, 30);
    cr.ShowText("This is some text, drawn with cairo");
    
    cr.MoveTo(40, 60);
    cr.ShowText("Powered by gir.core - GObject bindings for .NET");
};
window.ShowAll();

window.OnDestroy += (o,e) => Functions.MainQuit();

Functions.Main();
