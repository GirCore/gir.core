using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TextEditor.Document
{
    internal class PieceTable : IEnumerable<Node>
    {
        // TODO: Using Find() is O(n) -> We should store the information in the descriptor
        // itself and avoid lookup entirely. Consider using a custom LinkedList implementation.

        // See: https://www.catch22.net/tuts/piece-chains
        // Also: https://code.visualstudio.com/blogs/2018/03/23/text-buffer-reimplementation

        // These two are "sentinel" nodes which
        // do not store or point to data
        private readonly Node head;
        private readonly Node tail;
        
        internal class BoundaryException : Exception {}

        public PieceTable()
        {
            head = Node.CreateGuardNode();
            tail = Node.CreateGuardNode();
                
            head.Next = tail;
            tail.Prev = head;
        }

        private void CheckGuardNode(Node node)
        {
            if (node == head || node == tail)
                throw new BoundaryException();
        }

        public void AddAfter(Node current, Node insert)
        {
            CheckGuardNode(current);
            CheckGuardNode(insert);
            
            // [Current] [After] -> [Current] [Insert] [After]

            Node after = current.Next;

            insert.Next = after;
            insert.Prev = current;

            current.Next = insert;
            after!.Prev = insert;
        }

        public void AddBefore(Node current, Node insert)
        {
            CheckGuardNode(current);
            CheckGuardNode(insert);
            
            // [Before] [Current] -> [Before] [Insert] [Current]

            Node before = current.Prev;

            insert.Prev = before;
            insert.Next = current;

            before!.Next = insert;
            current.Prev = insert;
        }

        public void AddFirst(Node insert)
        {
            CheckGuardNode(insert);

            // [Head Sentinel] [After] -> [Head Sentinel] [Insert] [After] 

            Node after = head.Next;
            
            insert.Next = after;
            insert.Prev = head;
            
            head.Next = insert;
            after!.Prev = insert;
        }

        public void AddLast(Node insert)
        {
            CheckGuardNode(insert);

            // [Before] [Tail Sentinel] -> [Before] [Insert] [Tail Sentinel] 

            Node before = tail.Prev;
            
            insert.Next = tail;
            insert.Prev = before;
            
            tail.Prev = insert;
            before!.Next = insert;
        }

        public void Replace(Node old, Node replace)
        {
            CheckGuardNode(old);
            CheckGuardNode(replace);

            // [Before] [Old] [After] -> [Before] [Replace] [After]

            Node before = old.Prev;
            Node after = old.Next;

            replace.Prev = before;
            replace.Next = after;

            before!.Next = replace;
            after!.Prev = replace;
        }

        public void Remove(Node remove)
        {
            CheckGuardNode(remove);
            
            // [Before] [Remove] [After] -> [Before] [After]
            
            Node before = remove.Prev;
            Node after = remove.Next;

            before!.Next = after;
            after!.Prev = before;
        }

        // Implement IEnumerable
        public IEnumerator<Node> GetEnumerator()
        {
            var nodes = new List<Node>();
            var cur = head;
            while (cur != null)
            {
                nodes.Add(cur);
                cur = cur.Next;
                
                // We have traversed the entire list
                if (cur == null)
                    break;
                
                // Check for broken linked list -> Abort
                Debug.Assert(
                    condition: cur.Prev != null,
                    message: "Linked list has missing previous element. It may be broken"
                );
            }

            nodes.Remove(head);
            nodes.Remove(tail);

            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
