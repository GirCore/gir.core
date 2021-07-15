using cairo;
using Gtk;

namespace TextEditor.Application
{
    using Document;
    
    public class PieceTableDisplay : Bin
    {
        private Document doc;
        private DrawingArea drawingArea;
        public PieceTableDisplay(Document document)
        {
            doc = document;
            doc.DocumentChanged += (_,_) => QueueDraw();

            drawingArea = DrawingArea.New();
            drawingArea.OnDraw += Render;
            Child = drawingArea;
            ShowAll();
        }

        private void Render(object sender, DrawSignalArgs args)
        {
            Context cr = args.Cr;
            
            double xPos = 40;
            double yPos = 120;
            int padding = 5;

            RenderHeading(cr, xPos);

            cr.FontExtents(out FontExtents fontExtents);
            cr.SetFontSize(14);

            double position = xPos;
            foreach (Node node in doc.Contents)
            {
                // Get text for node
                string text = doc.RenderNode(node);
                
                // DRAW: Buffer Rectangle
                cr.MoveTo(position, yPos);
                
                cr.TextExtents(text, out TextExtents lineExtents);
                var length = lineExtents.xAdvance;
                var height = fontExtents.Height;

                // Set colour
                if (node.location == BufferType.File)
                    cr.SetSourceRgba(1, 0, 0, 1);
                else
                    cr.SetSourceRgba(0, 0, 1, 1);
                
                cr.Rectangle(position, yPos + fontExtents.Descent, length, height);
                cr.Fill();
                
                // DRAW: Buffer Text
                cr.MoveTo(position, yPos);
                cr.SetSourceRgba(0,0,0,1);
                cr.ShowText(text);
                
                // Move to next position - TODO: line wrap?
                position += length + padding;
            }
        }
        
        private void RenderHeading(Context cr, double xPos)
        {
            cr.MoveTo(xPos, 30);
            cr.SetFontSize(16);
            cr.ShowText("Piece Table Visualisation");
            
            // Red Square
            cr.SetSourceRgba(1,0,0,1);
            cr.Rectangle(xPos, 50, 10, -10);
            cr.Fill();
            
            // Blue Square
            cr.SetSourceRgba(0,0,1,1);
            cr.Rectangle(xPos, 70, 10, -10);
            cr.Fill();
            
            // Labels
            cr.SetFontSize(10);
            cr.SetSourceRgba(0,0,0,1);
            
            cr.MoveTo(xPos + 12, 50);
            cr.ShowText("File Buffer");
            
            cr.MoveTo(xPos + 12, 70);
            cr.ShowText("Add Buffer");
        }
    }
}
