﻿using System;
using System.Reflection;
using GLib;
using GObject;
using Gtk;
using Type = GObject.Type;

namespace GtkDemo
{
    public class CompositeWidget : Bin
    {
        private Button Button = default!;

        private static void ClassInit(Type gClass, System.Type type, IntPtr classData)
        {
            var templateData = Assembly.GetExecutingAssembly().ReadResourceAsByteArray("CompositeWidget.ui");
            SetTemplate(
                gtype: gClass,
                template: Bytes.From(templateData)
            );
            BindTemplateChild(gClass, nameof(Button));
            BindTemplateSignals(gClass, type);
        }

        protected override void Initialize()
        {
            InitTemplate();
            ConnectTemplateChildToField(nameof(Button), ref Button);
        }

        private void button_clicked(Button sender, System.EventArgs args)
        {
            sender.Label = "Clicked!";
        }
    }
}
