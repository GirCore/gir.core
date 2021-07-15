using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TextEditor.Document
{
    public abstract class Buffer
    {
        protected string _data = string.Empty;

        public string GetString(int index, int length)
            => _data.Substring(index, length);
    }

    public class ReadOnlyBuffer : Buffer
    {
        public ReadOnlyBuffer(string initData)
            => _data = initData;
    }

    public class AppendBuffer : Buffer
    {
        public AppendBuffer() {}
        
        public int Append(string text)
        {
            var index = _data.Length;
            _data += text;
            
            return index;
        }
    }
}
