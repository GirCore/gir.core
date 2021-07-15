using System;
using GirLoader.Output.Model;
using String = GirLoader.Output.Model.String;
using Type = GirLoader.Output.Model.Type;

namespace Generator
{
    internal static class Convert
    {
        internal static string ManagedToNative(TransferableAnyType transferable, string fromParam, Namespace currentNamespace)
        {
            Transfer transfer = transferable.Transfer;
            TypeInformation typeInfo = transferable.TypeInformation;
            Type type = transferable.TypeReference.ResolvedType
                        ?? throw new NullReferenceException($"Error: Conversion of '{fromParam}' to {nameof(Target.Native)} failed - {transferable.GetType().Name} does not have a type");

            var qualifiedNativeType = type.Write(Target.Native, currentNamespace);
            var qualifiedManagedType = type.Write(Target.Managed, currentNamespace);

            return (symbol: type, typeInfo) switch
            {
                // String Handling
                // String Arrays which do not have a length index need to be marshalled as IntPtr
                (String s, { Array: { Length: null } }) when transfer == Transfer.None => $"new GLib.Native.StringArrayNullTerminatedSafeHandle({fromParam}).DangerousGetHandle()",
                (String s, _) when (transfer == Transfer.None) && (transferable is ReturnValue) => $"GLib.Native.StringHelper.StringToHGlobalUTF8({fromParam})",

                // All other string types can be marshalled directly
                (String, _) => fromParam,

                //Pointed Record Conversions 
                (Record, { IsPointer: true, Array: null }) => $"{fromParam}.Handle",
                (Record, { IsPointer: true, Array: { } }) => $"{fromParam}.Select(x => x.Handle.DangerousGetHandle()).ToArray()",

                //Unpointed Record Conversions are not yet supported
                (Record r, { IsPointer: false, Array: null }) => $"({r.Repository.Namespace}.Native.{r.GetMetadataString("StructRefName")}) default!; //TODO: Fixme",
                (Record r, { IsPointer: false, Array: { } }) => $"({r.Repository.Namespace}.Native.{r.GetMetadataString("StructRefName")}[]) default!; //TODO: Fixme",
                
                // Class Conversions
                (Class { IsFundamental: true } c, { IsPointer: true, Array: null }) => $"{qualifiedManagedType}.To({fromParam})",
                (Class c, { IsPointer: true, Array: null }) => $"{fromParam}.Handle",
                (Class c, { IsPointer: true, Array: { } }) => throw new NotImplementedException($"Can't create delegate for argument {fromParam}"),
                (Class c, { Array: { } }) => $"{fromParam}.Select(cls => cls.Handle).ToArray()",

                // Interface Conversions
                (Interface i, { Array: { } }) => $"{fromParam}.Select(iface => (iface as GObject.Object).Handle).ToArray()",
                (Interface i, _) => $"({fromParam} as GObject.Object).Handle",

                // Other -> Try a brute-force cast
                (_, { Array: { } }) => $"({qualifiedNativeType}[]){fromParam}",
                _ => $"({qualifiedNativeType}){fromParam}"
            };
        }

        internal static string NativeToManaged(TransferableAnyType transferable, string fromParam, Namespace currentNamespace, bool useSafeHandle = true)
        {
            Transfer transfer = transferable.Transfer;
            TypeInformation typeInfo = transferable.TypeInformation;
            Type type = transferable.TypeReference.ResolvedType
                        ?? throw new NullReferenceException($"Error: Conversion of '{fromParam}' to {nameof(Target.Managed)} failed - {transferable.GetType().Name} does not have a type");

            var qualifiedType = type.Write(Target.Managed, currentNamespace);

            return (symbol: type, typeInfo) switch
            {
                // String Handling
                (String s, { Array: { Length: null } }) when transfer == Transfer.None => $"GLib.Native.StringHelper.ToStringArrayUtf8({fromParam})",
                (String s, _) when (transfer == Transfer.None) && (transferable is ReturnValue) => $"GLib.Native.StringHelper.ToStringUtf8({fromParam})",

                // Record Conversions (safe handles)
                (Record r, { IsPointer: true, Array: null }) when useSafeHandle => $"{r.Write(Target.Managed, currentNamespace)}.__FactoryNew({fromParam})",
                (Record r, { IsPointer: true, Array: { } }) when useSafeHandle => $"{fromParam}.Select(x => {r.Write(Target.Managed, currentNamespace)}.__FactoryNew(x)).ToArray()",

                // Record Conversions (raw pointers)
                (Record r, {IsPointer: true, Array: null }) when !useSafeHandle => $"{r.Write(Target.Managed, currentNamespace)}.__FactoryNew(new {SafeHandleFromRecord(r)}({fromParam}))",
                (Record r, {IsPointer: true, Array: { } }) when !useSafeHandle => $"{fromParam}.Select(x => {r.Write(Target.Managed, currentNamespace)}.__FactoryNew(new {SafeHandleFromRecord(r)}(x))).ToArray()",

                //Record Conversions without pointers are not working yet
                (Record r, {IsPointer: false, Array: null}) => $"({qualifiedType}) default!; //TODO: Fixme",
                (Record r, {IsPointer: false, Array: {}}) => $"({qualifiedType}[]) {fromParam}.Select(x => {qualifiedType}.__FactoryNew({SafeHandleFromRecord(r, true)}(x))).ToArray();",
                
                // Class Conversions
                (Class { IsFundamental: true } c, { IsPointer: true, Array: null }) => $"{qualifiedType}.From({fromParam})",
                (Class c, { IsPointer: true, Array: null }) => $"GObject.Native.ObjectWrapper.WrapHandle<{qualifiedType}>({fromParam}, {transfer.IsOwnedRef().ToString().ToLower()})",
                (Class c, { IsPointer: true, Array: { } }) => throw new NotImplementedException($"Can't create delegate for argument '{fromParam}'"),

                // Misc
                (Interface i, _) => $"GObject.Native.ObjectWrapper.WrapHandle<{qualifiedType}>({fromParam}, {transfer.IsOwnedRef().ToString().ToLower()})",
                (Union u, _) => $"",

                // Other -> Try a brute-force cast
                (_, { Array: { } }) => $"({qualifiedType}[]){fromParam}",
                _ => $"({qualifiedType}){fromParam}"
            };
        }

        private static string SafeHandleFromRecord(Record r, bool managedHandle = false)
        {
            var type = r.GetMetadataString(managedHandle ? "SafeHandleRefManagedFunc" : "SafeHandleRefName");
            var nspace = $"{r.Repository.Namespace}.Native";
            return nspace + "." + type;
        }
    }
}
