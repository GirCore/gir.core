using System;
using Gir;

namespace Generator
{
    internal enum ResolverResult
    {
        Found = 0,
        NotFound = 1,
        NotSupported = 2
    }

    public class TypeResolver
    {
        private readonly AliasResolver aliasResolver;

        public TypeResolver(AliasResolver resolver)
        {
            this.aliasResolver = resolver;
        }

        public string Resolve(IType typeInfo) => typeInfo switch
        {
            { Type: { } gtype } => ResolveGType(gtype, typeInfo is GParameter),
            { Array: { Length: { } length, Type: { CType: { } } gtype } } => ResolveArrayType(gtype, typeInfo is GParameter, length),
            { Array: { } } => "IntPtr",
            _ => throw new NotSupportedException("Type is missing supported Type information")
        };

        private string ResolveArrayType(GType arrayType, bool isParameter, int length)
        {
            var type = ResolveGType(arrayType, isParameter);

            if (type == "string")
            {
                return "ref IntPtr";
            }
            else if (type != "IntPtr")
            {
                if (length > 0)
                {
                    type = type + "[]";

                    if (isParameter)
                        type = GetMarshal(length) + " " + type;
                }
                else
                {
                    return "IntPtr";
                }
            }

            return type;
        }

        private string ResolveGType(GType gtype, bool isParameter)
        {
            if (gtype.CType is null)
                throw new Exception("GType is missing CType");

            var ctype = gtype.CType;

            if (ctype.In("va_list", "GType", "gpointer", "gconstpointer"))
                return "IntPtr";

            if (aliasResolver.TryGetForCType(ctype, out var resolvedCType))
                ctype = resolvedCType;

            (var result, var typeName, var isPrimitive, var isPointer) = ResolveCType(ctype);

            return result switch
            {
                ResolverResult.NotFound => gtype.Name ?? throw new Exception($"GType {ctype} is missing a name"),
                ResolverResult.Found => FixTypeName(typeName, isParameter, isPointer, isPrimitive),
                _ => throw new Exception($"{ctype} is not supported")
            };
        }

        private string FixTypeName(string typeName, bool isParameter, bool isPointer, bool isPrimitive)
            => (typeName, isParameter, isPointer, isPrimitive) switch
            {
                ("string", true, _, _) => typeName, //string stays string for parameter values, they are marshalled automatically
                (_, _, true, true) => "ref " + typeName,
                (_, _, true, false) => "IntPtr",
                _ => typeName
            };

        private string GetMarshal(int arrayLength)
            => $"[MarshalAs(UnmanagedType.LPArray, SizeParamIndex={arrayLength})]";

        private (ResolverResult result, string Type, bool IsPrimitive, bool IsPointer) ResolveCType(string cType)
        {
            var isPointer = cType.EndsWith("*");
            cType = cType.Replace("*", "");

            var result = cType switch
            {
                var t when isPointer && t == "void" => IntPtr(),
                var t when isPointer && t == "JSCValue" => IntPtr(),

                "void" => Primitive("void"),
                "gboolean" => Primitive("bool"),
                "gfloat" => Primitive("float"),

                "GCallback" => Complex("Delegate"), // Signature of a callback is determined by the context in which it is used

                var t when isPointer && t == "guchar" => String(),
                var t when isPointer && t == "gchar" => String(),
                var t when isPointer && t == "const gchar" => String(),
                var t when isPointer && t == "const char" => String(),
                var t when isPointer && t == "char" => String(),

                "gconstpointer" => IntPtr(),
                "va_list" => IntPtr(),
                "gpointer" => IntPtr(),
                "GType" => IntPtr(),
                "tm" => IntPtr(),

                "GValue" => Value(),
                "const GValue" => Value(),

                "guint16" => UShort(),
                "gushort" => UShort(),

                "gint16" => Short(),
                "gshort" => Short(),

                "gdouble" => Double(),
                "long double" => Double(),

                "int" => Int(), //Workaround: There are aliases which do not return "g datatypes" but the native ones. In this case the type resolver would return "not found" and the native type would not be used!
                "gint" => Int(),
                "gint32" => Int(),

                "guint" => UInt(),
                "guint32" => UInt(),
                var t when isPointer && t == "const guint32" => UInt(),
                "GQuark" => UInt(),
                "gunichar" => UInt(),
                "const gunichar" => UInt(),

                "guint8" => Byte(),
                "gint8" => Byte(),
                "gchar" => Byte(),
                "guchar" => Byte(),
                var t when isPointer && t == "const guint8" => Byte(),

                "glong" => Long(),
                "gssize" => Long(),
                "gint64" => Long(),
                "goffset" => Long(),
                "time_t" => Long(),

                "gsize" => ULong(),
                "guint64" => ULong(),
                "gulong" => ULong(),
                "Window" => ULong(),

                var t when t.StartsWith("Atk") => NotSupported(t),
                var t when t.StartsWith("Cogl") => NotSupported(t),

                _ => NotFound()
            };

            return (result.reslt, result.Type, result.IsPrimitive, isPointer);
        }

        private (ResolverResult reslt, string Type, bool IsPrimitive) String()
            => Complex("string");
        private (ResolverResult reslt, string Type, bool IsPrimitive) IntPtr()
            => Complex("IntPtr");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Value()
            => Primitive("GObject.Value");
        private (ResolverResult reslt, string Type, bool IsPrimitive) UShort()
            => Primitive("ushort");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Short()
            => Primitive("short");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Double()
            => Primitive("double");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Int()
            => Primitive("int");
        private (ResolverResult reslt, string Type, bool IsPrimitive) UInt()
            => Primitive("uint");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Byte()
            => Primitive("byte");
        private (ResolverResult reslt, string Type, bool IsPrimitive) Long()
            => Primitive("long");
        private (ResolverResult reslt, string Type, bool IsPrimitive) ULong()
            => Primitive("ulong");

        private (ResolverResult reslt, string Type, bool IsPrimitive) Primitive(string str)
            => (ResolverResult.Found, str, true);
        private (ResolverResult reslt, string Type, bool IsPrimitive) Complex(string str)
            => (ResolverResult.Found, str, false);

        private (ResolverResult reslt, string Type, bool IsPrimitive) NotSupported(string str)
            => (ResolverResult.NotSupported, "", false);
        private (ResolverResult reslt, string Type, bool IsPrimitive) NotFound()
            => (ResolverResult.NotFound, "", false);

    }
}
