using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TextEditor.Document
{
    public enum BufferType : byte
    {
        // For guard nodes
        None,
        
        // Original file buffer
        File,
        
        // Append-only add buffer
        Add
    };

    // Also called: Span, Piece, Descriptor, etc
    public class Node
    {
        internal Node? Next { get; set; }
        
        internal Node? Prev { get; set; }
        
        internal BufferType location { get; init; }
        internal int offset { get; init; }
        internal int length { get; init; }

        public Node(BufferType location, int offset, int length)
        {
            this.location = location;
            this.offset = offset;
            this.length = length;
        }

        internal static Node CreateGuardNode()
            => new Node(BufferType.None, 0, 0);
    }
}
