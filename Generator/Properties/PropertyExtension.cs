using System;
using Repository.Model;
using String = Repository.Model.String;

namespace Generator.Properties
{
    internal static class PropertyExtension
    {
        internal static string GetDescriptorName(this Property property)
            => property.SymbolName + "PropertyDefinition";
        
        internal static string GetTypeName(this Property property, Namespace currentNamespace)
        {
            var type = property.WriteType(Target.Managed, currentNamespace);

            if (property.SymbolReference.Symbol is null)
                throw new Exception($"Can not get type of property {property.Name}. Symbol is missing");
            
            // Properties need special nullable handling as each C value is implicitly nullable.
            // The C# Marshaller returns default values for primitive value types. But there
            // can be null values for strings and objects, even if the nullability attribute
            // is not present in the gir file
            return property.SymbolReference.Symbol switch
            {
                String => type + "?",
                Class => type + "?",
                _ => type
            };
        }
    }
}
