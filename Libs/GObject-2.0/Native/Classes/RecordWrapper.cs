using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using GLib;

namespace GObject.Native
{
    public class RecordWrapper
    {
        public static object WrapHandle(IntPtr handle, Type gtype)
        {
            System.Type trueType = TypeDictionary.GetSystemType(gtype);
            
            if (handle == IntPtr.Zero)
                throw new NullReferenceException($"Failed to wrap handle as type <{trueType}>. Null handle passed to WrapHandle.");

            // Get constructor for the true type
            MethodInfo? factory = GetRecordFactory(trueType);

            if (factory == null)
                throw new Exception($"Type {trueType} does not define an IntPtr constructor. This could mean improperly defined bindings");

            object? result = factory.Invoke(null, new object[] { handle });
            
            if (result == null)
                throw new Exception($"Type {trueType}'s factory method returned a null object. This could mean improperly defined bindings");

            return result;
        }
        
        private static MethodInfo? GetRecordFactory(System.Type type)
        {
            // Create using 'IntPtr' constructor
            MethodInfo? factory = type.GetMethod("__FactoryNew",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static,
                null, new[] { typeof(IntPtr) }, null
            );
            return factory;
        }
    }
}
