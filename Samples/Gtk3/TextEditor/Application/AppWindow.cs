using System;
using Gtk;

namespace TextEditor.Application
{
    using Document;

    public class AppWindow : Gtk.ApplicationWindow
    {
        const string WindowTitle = "Editor";

        private Paned contentPaned;
        private Entry insertEntry;
        private SpinButton spinButton;
        private DocumentView documentView;

        public Document Document { get; set; }

        public AppWindow()
        {
            this.DefaultWidth = 800;
            this.DefaultHeight = 600;

            // Create UI
            var headerBar = HeaderBar.New();
            headerBar.ShowCloseButton = true;
            headerBar.Title = WindowTitle;
            this.SetTitlebar(headerBar);
            this.SetDecorated(true);
            
            // About Dialog Button
            var aboutButton = new Button("About");
            aboutButton.Image = Image.NewFromIconName("dialog-information-symbolic", IconSize.Button);
            aboutButton.AlwaysShowImage = true;
            aboutButton.OnClicked += (_, _) =>
            {
                // This is implemented in the 'AboutDialog' sample. You can
                // safely substitute it for the built-in GTK AboutDialog.
                var dlg = new AboutDialog.SampleDialog("Text Editor");
                
                dlg.SetTransientFor(this);
                dlg.SetModal(true);
                dlg.Present();
            };
            headerBar.PackEnd(aboutButton);

            // Inspector Button
            var inspectButton = new Button("Inspector");
            inspectButton.Image = Image.NewFromIconName("edit-find-symbolic", IconSize.Button);
            inspectButton.AlwaysShowImage = true;
            inspectButton.OnClicked += (_, _) =>
            {
                SetInteractiveDebugging(true);
            };
            headerBar.PackEnd(inspectButton);

            // Main Content Pane
            contentPaned = Paned.New(Orientation.Vertical);
            this.Child = contentPaned;

            // Start by creating a document
            Document = Document.NewFromString("The quick brown fox jumps over the lazy dog.");
            documentView = new DocumentView(Document);

            var display = new PieceTableDisplay(Document);
            
            // Paned
            contentPaned.Add1(documentView);
            contentPaned.Add2(display);
            contentPaned.Position = 400;
            
            // Set visible
            headerBar.ShowAll();
            contentPaned.ShowAll();
        }

        void UpdateCursor()
        {
            var value = Math.Clamp((int)spinButton.GetValue(), 0, Int32.MaxValue-1);
            documentView.SetCursorIndex(value);
        }

        void Insert(Button button, EventArgs args)
        {
            var index = spinButton.GetValue();
            var text = insertEntry.GetText();
            Document.Insert((int)index, text);

            // Move cursor to end of insertion
            spinButton.Value += text.Length;
        }

        void Delete(Button button, EventArgs args)
        {
            var index = (int)spinButton.GetValue();
            Document.Delete(index, 1);
        }
    }
}
