using System.Xml.Serialization;

namespace Repository.Xml
{
    public class MethodInfo : CallableAttributes
    {
        [XmlAttribute(CallableAttributes.NameAttribute)]
        public string? Name { get; set; }

        [XmlAttribute(CallableAttributes.IdentifierAttribute, Namespace = CallableAttributes.IdentifierNamespace)]
        public string? Identifier { get; set; }

        [XmlAttribute(CallableAttributes.ThrowsAttribute)]
        public bool Throws { get; set; }

        [XmlAttribute(InfoAttributes.DeprecatedAttribute)]
        public bool Deprecated { get; set; }

        [XmlAttribute(InfoAttributes.DeprecatedVersionAttribute)]
        public string? DeprecatedVersion { get; set; }

        [XmlAttribute(InfoAttributes.IntrospectableAttribute)]
        public bool Introspectable { get; set; }

        [XmlElement("return-value")]
        public ReturnValueInfo? ReturnValue { get; set; }

        [XmlElement("doc")]
        public DocInfo? Doc { get; set; }

        [XmlElement("doc-deprecated")]
        public DocInfo? DocDeprecated { get; set; }

        [XmlElement("parameters")]
        public ParametersInfo? Parameters { get; set; }

        [XmlAttribute(CallableAttributes.MovedToAttribute)]
        public string? MovedTo { get; set; }

        public override string? ToString()
            => Name ?? Identifier ?? base.ToString();
    }
}
