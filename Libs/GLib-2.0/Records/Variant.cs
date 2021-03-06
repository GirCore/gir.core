﻿using System;
using System.Linq;
using GLib.Native;

namespace GLib
{
    public partial class Variant : IDisposable
    {
        #region Fields

        private Variant[] _children;

        #endregion

        #region Constructors

        public Variant(params Variant[] children)
        {
            _children = children;
            Init(out _handle, children);
        }

        /*public Variant(IDictionary<string, Variant> dictionary)
        {
            var data = new Variant[dictionary.Count];
            var counter = 0;
            foreach(var entry in dictionary)
            {
                var e = new Variant(Variant.new_dict_entry(new Variant(entry.Key).Handle, entry.Value.handle));
                data[counter] = e;
                counter++;
            }
            this.children = data;
            Init(out this.handle, data);
        }*/

        partial void Initialize()
        {
            _children = new Variant[0];
            Native.Variant.Methods.RefSink(_handle);
        }

        #endregion

        #region Methods

        public static Variant Create(int i) => new Variant(Native.Variant.Methods.NewInt32(i));
        public static Variant Create(uint ui) => new Variant(Native.Variant.Methods.NewUint32(ui));
        public static Variant Create(string str) => new Variant(Native.Variant.Methods.NewString(str));
        public static Variant Create(params string[] strs) => new Variant(Native.Variant.Methods.NewStrv(strs, strs.Length));

        public static Variant CreateEmptyDictionary(VariantType key, VariantType value)
        {
            var childType = Native.VariantType.Methods.NewDictEntry(key.Handle, value.Handle);
            return new Variant(Native.Variant.Methods.NewArray(childType, new IntPtr[0], 0));
        }

        private void Init(out Native.Variant.Handle handle, params Variant[] children)
        {
            _children = children;

            var count = (nuint) children.Length;
            var ptrs = new IntPtr[count];

            for (nuint i = 0; i < count; i++)
                ptrs[i] = children[i].Handle.DangerousGetHandle();

            handle = Native.Variant.Methods.NewTuple(ptrs, count);
            Native.Variant.Methods.RefSink(handle);
        }

        public string GetString()
            => StringHelper.ToStringUtf8(Native.Variant.Methods.GetString(_handle, out _));

        public int GetInt()
            => Native.Variant.Methods.GetInt32(_handle);

        public uint GetUInt()
            => Native.Variant.Methods.GetUint32(_handle);

        public string Print(bool typeAnnotate)
            => Native.Variant.Methods.Print(_handle, typeAnnotate);

        #endregion

        public void Dispose()
        {
            foreach (var child in _children)
                child.Dispose();

            Handle.Dispose();
        }
    }

    public static class VariantExtension
    {
        public static Native.Variant.Handle GetSafeHandle(this Variant? variant)
            => variant is null ? Native.Variant.Handle.Null : variant.Handle;
    }
}
