using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GObject.Native
{
    /// <summary>
    /// This exception is thrown when a <see cref="System.Type"/> is not found in
    /// the type dictionary.
    /// </summary>
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException(System.Type managedType)
            : base($"Type {managedType.FullName} not registered in type dictionary") { }
    }

    internal enum BasicType
    {
        Invalid = 0 << 2,
        None = 1 << 2,
        Interface = 2 << 2,
        Char = 3 << 2,
        UChar = 4 << 2,
        Boolean = 5 << 2,
        Int = 6 << 2,
        UInt = 7 << 2,
        Long = 8 << 2,
        ULong = 9 << 2,
        Int64 = 10 << 2,
        UInt64 = 11 << 2,
        Enum = 12 << 2,
        Flags = 13 << 2,
        Float = 14 << 2,
        Double = 15 << 2,
        String = 16 << 2,
        Pointer = 17 << 2,
        Boxed = 18 << 2,
        Param = 19 << 2,
        Object = 20 << 2,
        Variant = 21 << 2
    }

    /// <summary>
    /// The global type dictionary which maps between the .NET Type System and
    /// the GType dynamic type system.
    /// </summary>
    public static class TypeDictionary
    {
        private static readonly Dictionary<System.Type, Type> _systemTypeDict = new();
        private static readonly Dictionary<Type, System.Type> _reverseTypeDict = new();

        /// <summary>
        /// Add a new mapping of (System.Type, GObject.Type) to the type dictionary. 
        /// </summary>
        /// <param name="systemType">A managed type that has not already been registered</param>
        /// <param name="type">The gtype retrieved from the object's get type method or from registration.</param>
        public static void Add(System.Type systemType, Type type)
        {
            // Check we have not already registered
            Debug.Assert(
                condition: !_systemTypeDict.ContainsKey(systemType),
                message: $"Type {nameof(systemType)} is already registered in the type dictionary."
            );

            _systemTypeDict[systemType] = type;
            _reverseTypeDict[type] = systemType;
        }

        /// <summary>
        /// For a given managed GObject-based class, retrieve the corresponding gtype.
        /// </summary>
        /// <param name="type">The type of a class that is equal or derived from <see cref="GObject.Object"/></param>
        /// <returns>The equivalent GType</returns>
        /// <exception cref="TypeNotFoundException">The given type is not registered in the type dictionary. The caller should register it themselves.</exception>
        internal static Type GetGType(System.Type type)
        {
            // Check we are a GObject
            Debug.Assert(
                condition: type.IsAssignableTo(typeof(GObject.Object)),
                message: $"Parameter {type} is not a GObject or subclass of GObject"
            );

            // Throw if type is not registered
            if (!_systemTypeDict.ContainsKey(type))
                throw new TypeNotFoundException(type);

            return _systemTypeDict[type];
        }

        /// <summary>
        /// For a given gtype, retrieve the corresponding managed type.  
        /// </summary>
        /// <param name="gtype">A type from the GType type system</param>
        /// <returns>The equivalent managed type</returns>
        internal static System.Type GetSystemType(Type gtype)
        {
            if (_reverseTypeDict.TryGetValue(gtype, out System.Type? sysType))
                return sysType;

            if (Functions.TypeIsA(gtype.Value, Type.Boxed.Value))
                throw new Exception($"Record not registered: {gtype.ToString()}");

            // If gtype is not in the type dictionary, walk up the
            // tree until we find a type that is. As all objects are
            // descended from GObject, we will eventually find a parent
            // type that is registered.

            while (!_reverseTypeDict.TryGetValue(gtype, out sysType))
            {
                gtype = new Type(Functions.TypeParent(gtype.Value));
                if (gtype.Value == (nuint) BasicType.Invalid ||
                    gtype.Value == (nuint) BasicType.None)
                    throw new Exception("Could not retrieve parent type - is the typeid valid?");
            }

            // Store for future lookups
            _reverseTypeDict[gtype] = sysType;

            return sysType;
        }

        // These may be unneeded - keep for now 
        internal static bool ContainsGType(Type gtype)
            => _reverseTypeDict.ContainsKey(gtype);

        internal static bool ContainsSystemType(System.Type type)
            => _systemTypeDict.ContainsKey(type);
    }
}
