using System.Xml.Serialization;

namespace Repository.Xml
{
    public class PropertyInfo : ITypeOrArray
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("writable")]
        public bool Writeable { get; set; }

        [XmlAttribute("construct")]
        public bool Construct { get; set; }

        [XmlAttribute("transfer-ownership")]
        public string? TransferOwnership { get; set; }

        [XmlAttribute("deprecated")]
        public bool Deprecated { get; set; }

        [XmlAttribute("deprecated-version")]
        public string? DeprecatedVersion { get; set; }

        [XmlElement("doc")]
        public DocInfo? Doc { get; set; }

        [XmlElement("doc-deprecated")]
        public DocInfo? DocDeprecated { get; set; }

        [XmlElement("type")]
        public TypeInfo? Type { get; set; }
        
        [XmlElement("array")]
        public ArrayInfo? Array { get; set; }
    }
}