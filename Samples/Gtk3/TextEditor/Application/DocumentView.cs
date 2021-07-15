using System;
using System.Text;
using System.Diagnostics;
using Gtk;

using cairo;
using Gdk;
using Pango;

namespace TextEditor.Application
{
    using Document;

    public class DocumentView : Gtk.Bin
    {
        private Document doc;
        private int cursorIndex;

        private string cachedText;

        private EventBox eventBox;

        private DrawingArea area;

        private IMContext context;

        public DocumentView(Document document)
        {
            // Setup IMContext
            context = IMContextSimple.New();
            context.OnCommit += OnEnterText;
            
            // Add event box to catch keyboard input
            eventBox = EventBox.New();
            eventBox.OnKeyPressEvent += OnKeydown;
            eventBox.CanFocus = true;
            eventBox.OnRealize += (o, e) => context.SetClientWindow(eventBox.GetWindow());
            Child = eventBox;

            // Drawing area to display rich text
            area = DrawingArea.New();
            area.OnDraw += Render;
            eventBox.Child = area;

            // Load data
            SetDocument(document);
            
            ShowAll();
        }

        public void SetDocument(Document document)
        {
            doc = document;
            doc.DocumentChanged += OnDocumentChanged;

            UpdateCache();
            Redraw();

            cursorIndex = 0;
        }

        public void SetCursorIndex(int index)
        {
            cursorIndex = Math.Clamp(index, 0, cachedText.Length);
            Redraw();
        }

        public void CloseDocument()
        {
            doc = null;
            cursorIndex = 0;
            cachedText = null;
        }

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            // The document has been modified - redraw
            // TODO: Redraw the currently on-screen section only
            Debug.Assert(sender == doc);
            UpdateCache();
            Redraw();
        }

        private void OnKeydown(object sender, KeyPressEventSignalArgs e)
        {
            Console.WriteLine("Keydown " + e.@event.Keyval);

            var keyVal = e.@event.Keyval;

            if (keyVal == Gdk.Constants.KEY_Left)
            {
                cursorIndex = Math.Clamp(cursorIndex - 1, 0, cachedText.Length);
                Redraw();
            }
            else if (keyVal == Gdk.Constants.KEY_Right)
            {
                cursorIndex = Math.Clamp(cursorIndex + 1, 0, cachedText.Length);
                Redraw();
            }
            else if (keyVal == Gdk.Constants.KEY_BackSpace)
                doc.Delete(Math.Clamp(cursorIndex - 1, 0, cachedText.Length), 1);
            else
                context.FilterKeypress(e.@event); // Check result?
        }

        private void OnEnterText(object sender, IMContext.CommitSignalArgs e)
        {
            doc.Insert(cursorIndex, e.Str);
            cursorIndex += e.Str.Length;
        }

        private void UpdateCache()
        {
            var builder = new StringBuilder();

            // Assemble the document
            foreach (Node textSpan in doc.Contents)
                builder.Append(doc.RenderNode(textSpan));

            cachedText = builder.ToString();
        }

        private void Redraw()
        {
            // textLayout.LabelProp = cachedText.Insert(cursorIndex, "|");
            area.QueueDraw();
        }

        private void Render(object sender, DrawingArea.DrawSignalArgs e)
        {
            Debug.Assert(area == sender);

            int xDist = 30;
            int yDist = 100;

            cairo.Context cr = e.Cr;
            
            // Fill background
            cr.SetSourceRgba(1,1,1,1);
            cr.Rectangle(0, 0, GetAllocatedWidth(), GetAllocatedHeight());
            cr.Fill();

            // Write Info Text
            cr.SetSourceRgba(0,0,0,1.0);
            cr.MoveTo(xDist, 30);
            cr.ShowText("This sample uses Cairo and a Gtk.DrawingArea to create a simple 'TextView' clone.");
            cr.MoveTo(xDist, 50);
            cr.ShowText("Use the LEFT and RIGHT arrow keys to navigate. Press BACKSPACE to delete.");
            
            // Draw Cursor
            cr.SetFontSize(16);
            
            cr.TextExtents(cachedText, out TextExtents lineExtents);
            var height = lineExtents.height;
            
            cr.TextExtents(cachedText.Substring(0, cursorIndex), out TextExtents cursorExtents);
            var xPos = cursorExtents.width;

            cr.FontExtents(out FontExtents fontExtents);
            
            cr.SetSourceRgba(1.0, 0, 0, 1.0);
            cr.Rectangle(xDist + cursorExtents.xAdvance - 0.5, yDist + fontExtents.Descent, 1, -(fontExtents.Descent + fontExtents.Ascent));
            cr.Fill();
            
            // Draw Text from Buffer
            cr.MoveTo(xDist, yDist);
            cr.SetSourceRgba(0, 0, 0, 1.0);
            cr.ShowText(cachedText);
        }
    }
}
