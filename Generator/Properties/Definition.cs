using System.Text;
using Repository.Model;

namespace Generator.Properties
{
    internal class Definition : Base
    {
        private readonly Property _property;
        private readonly Namespace _currentNamespace;

        public Definition(Property property, Namespace currentNamespace)
        {
            _property = property;
            _currentNamespace = currentNamespace;
        }
        
        public string Write()
        {
            var descriptorName = _property.GetDescriptorName();
            var typeName = _property.GetTypeName(_currentNamespace);

            var builder = new StringBuilder();
            builder.AppendLine($"public {typeName} {_property.SymbolName}");
            builder.AppendLine("{");

            if (_property.Readable)
                builder.AppendLine($"    get => GetProperty({descriptorName});");

            if (_property.Writeable)
                builder.AppendLine($"    set => SetProperty({descriptorName}, value);");

            builder.AppendLine("}");

            return CommentIfUnsupported(builder.ToString(), _property);
        }
    }
}
