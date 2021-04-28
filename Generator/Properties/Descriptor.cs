using System;
using System.Collections.Generic;
using Repository.Model;

namespace Generator.Properties
{
    internal class Descriptor : Base
    {
        private readonly Property _property;
        private readonly Symbol _symbol;
        private readonly Namespace _currentNamespace;

        public Descriptor(Property property, Symbol symbol, Namespace currentNamespace)
        {
            _property = property;
            _symbol = symbol;
            _currentNamespace = currentNamespace;
        }
        
        public string Write()
        {
            if (_symbol.Namespace is null)
                throw new Exception("Can not write property for symbol with unknown namespace");

            var typeName = _property.GetTypeName(_currentNamespace);
            var descriptorName = _property.GetDescriptorName();
            var parentType = _symbol.Write(Target.Managed, _currentNamespace);

            var definition = @$"public static readonly Property<{typeName}> {descriptorName} = Property<{typeName}>.Register<{parentType}>(
    {GetArguments(_property)}
);
";
            return CommentIfUnsupported(definition, _property);
        }

        private static string GetArguments(Property property)
        {
            var arguments = new List<string>()
            {
                $"nativeName: \"{property.Name}\"",
                $"managedName: nameof({property.SymbolName})"
            };

            if (property.Readable)
                arguments.Add($"get: o => o.{property.SymbolName}");

            if (property.Writeable)
                arguments.Add($"set: (o, v) => o.{property.SymbolName} = v");

            return string.Join(",\r\n    ", arguments);
        }
    }
}
