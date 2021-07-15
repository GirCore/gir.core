using System;
using GirLoader;
using GirLoader.Output.Model;
using String = GirLoader.Output.Model.String;

namespace Generator
{
    internal static class AnyTypeExtension
    {
        internal static string WriteType(this AnyType anyType, Target target, Namespace currentNamespace, bool useSafeHandle = true)
        {
            return target switch
            {
                Target.Managed => WriteManagedType(anyType, currentNamespace, useSafeHandle),
                Target.Native => WriteNativeType(anyType, currentNamespace, useSafeHandle)
            };
        }

        private static string WriteNativeType(AnyType anyType, Namespace currentNamespace, bool useSafeHandle = true)
        {
            return anyType switch
            {
                { TypeReference: { ResolvedType: Callback c } } => c.Write(Target.Native, currentNamespace),

                // Return values which return a string without transferring ownership to us can not be marshalled automatically
                // as the marshaller want's to free the unmanaged memory which is not allowed if the ownership is not transferred
                ReturnValue { Transfer: Transfer.None, TypeReference: { ResolvedType: String } } => "IntPtr",

                // Arrays of string which do not transfer ownership and have no length index can not be marshalled automatically
                TransferableAnyType { TypeInformation: { Array: { Length: null } }, TypeReference: { ResolvedType: String }, Transfer: Transfer.None } => "IntPtr",

                // Arrays of string can be marshalled automatically, no IntPtr needed
                { TypeInformation: { Array: { } }, TypeReference: { ResolvedType: String } } => "string[]",

                // Arrays of byte can be marshalled automatically, no IntPtr needed
                { TypeInformation: { Array: { } }, TypeReference: { ResolvedType: { } s } } when s.SymbolName == "byte" => "byte[]",

                //References to records which are not using a pointer
                {TypeReference: { ResolvedType: Record r}, TypeInformation: { IsPointer: false, Array: null}} => GetStructName(r, currentNamespace),
                {TypeReference: { ResolvedType: Record r}, TypeInformation: { IsPointer: false, Array: { }}} => GetStructName(r, currentNamespace) + "[]",
                
                //References to records which are using a pointer
                {TypeReference: { ResolvedType: Record r}, TypeInformation: { IsPointer: true, Array: null}} => GetSafeHandleName(r, currentNamespace, useSafeHandle),
                {TypeReference: { ResolvedType: Record r}, TypeInformation: { IsPointer: true, Array: { }}} => "IntPtr[]", //Array of SafeHandle not supported by runtime
                
                //References to records which are not using a pointer
                {TypeReference: { ResolvedType: Union u}, TypeInformation: { IsPointer: false, Array: null}} => GetStructName(u, currentNamespace),
                {TypeReference: { ResolvedType: Union u}, TypeInformation: { IsPointer: false, Array: { }}} => GetStructName(u, currentNamespace) + "[]",
                
                //References to records which are using a pointer
                {TypeReference: { ResolvedType: Union u}, TypeInformation: { IsPointer: true, Array: null}} => "IntPtr",
                {TypeReference: { ResolvedType: Union u}, TypeInformation: { IsPointer: true, Array: { }}} => "IntPtr[]",

                // Primitives - Marshal directly
                { TypeInformation: { Array: { } }, TypeReference: { ResolvedType: PrimitiveValueType s } } => s.Write(Target.Native, currentNamespace) + "[]",
                { TypeReference: { ResolvedType: PrimitiveValueType s } } => s.Write(Target.Native, currentNamespace),

                // Enumerations - Marshal directly
                { TypeReference: { ResolvedType: Enumeration }, TypeInformation: { Array: { } } } => anyType.TypeReference.ResolvedType.Write(Target.Native, currentNamespace) + "[]",
                { TypeReference: { ResolvedType: Enumeration } } => anyType.TypeReference.ResolvedType.Write(Target.Native, currentNamespace),

                // Use IntPtr for all types where a pointer is expected
                { TypeInformation: { IsPointer: true, Array: { Length: not null } } } => "IntPtr[]",
                { TypeInformation: { IsPointer: true } } => "IntPtr",

                { TypeInformation: { Array: { } } } => anyType.TypeReference.ResolvedType.Write(Target.Native, currentNamespace) + "[]",
                _ => anyType.TypeReference.ResolvedType.Write(Target.Native, currentNamespace)
            };
        }

        private static string GetStructName(Record r, Namespace currentNamespace)
        {
            return AddNamespace(currentNamespace, r.Repository.Namespace, r.GetMetadataString("StructRefName"), Target.Native);
        }
        
        private static string GetStructName(Union u, Namespace currentNamespace)
        {
            return AddNamespace(currentNamespace, u.Repository.Namespace, u.GetMetadataString("StructRefName"), Target.Native);
        }
        
        private static string GetSafeHandleName(Record r, Namespace currentNamespace, bool useSafeHandle)
        {
            if (useSafeHandle)
                return AddNamespace(currentNamespace, r.Repository.Namespace, r.GetMetadataString("SafeHandleRefName"), Target.Native);
            
            return "IntPtr";
        }

        private static string WriteManagedType(AnyType anyType, Namespace currentNamespace, bool useSafeHandle = true)
        {
            var result = anyType switch
            {
                { TypeReference: { ResolvedType: Callback c } } => AddNamespace(currentNamespace, c.Repository.Namespace, c.GetMetadataString("ManagedName"), Target.Managed),
                { TypeReference: { ResolvedType: Record r } } => AddNamespace(currentNamespace, r.Repository.Namespace, r.GetMetadataString("Name"), Target.Managed),
                { TypeReference: { ResolvedType: Union u } } => AddNamespace(currentNamespace, u.Repository.Namespace, u.GetMetadataString("Name"), Target.Managed),

                _ => anyType.TypeReference.ResolvedType.Write(Target.Managed, currentNamespace)
            };

            if (anyType.TypeInformation.Array is { })
                result += "[]";

            return result;
        }

        private static string AddNamespace(Namespace currentNamespace, Namespace? targetNamespace, string str, Target target)
        {
            if (!currentNamespace.IsForeignTo(targetNamespace))
                return str;

            if (targetNamespace is null)
                throw new Exception("Target namespace is missing");

            return targetNamespace.GetName(target) + "." + str;
        }
    }
}
