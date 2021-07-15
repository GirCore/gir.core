using System;
using GirLoader.Output.Model;
using Type = GirLoader.Output.Model.Type;

namespace Generator
{
    internal static class SymbolExtension
    {
        internal static string Write(this Type type, Target target, Namespace currentNamespace)
        {
            var name = type.SymbolName;

            var repositoryNamespace = type.Repository?.Namespace;
            if (!repositoryNamespace.IsForeignTo(currentNamespace))
                return name;

            if (type.Repository is null)
                throw new Exception($"Can not write {nameof(Type)}, because repository is missing");

            var namespaceName = type switch
            {
                //Enumerations do not have a native representation they always live in the managed namespace
                Enumeration => type.Repository.Namespace.Name,

                _ => type.Repository.Namespace.GetName(target)
            };

            return namespaceName + "." + name;
        }

        public static string WriteTypeRegistrationClass(this Type type)
        {
            return $"TypeDictionary.Add(typeof({type.SymbolName}), new GObject.Type(Native.{type.SymbolName}.Instance.Methods.GetGType()));\r\n";
        }
        
        public static string WriteTypeRegistrationBoxed(this Type type)
        {
            return $"TypeDictionary.Add(typeof({type.SymbolName}), new GObject.Type(Native.{type.SymbolName}.Methods.GetGType()));\r\n";
        }
    }
}
