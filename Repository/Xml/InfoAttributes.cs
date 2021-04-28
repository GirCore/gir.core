namespace Repository.Xml
{
    public interface InfoAttributes
    {
        const string DeprecatedAttribute = "deprecated";
        const string DeprecatedVersionAttribute = "deprecated-version";
        const string IntrospectableAttribute = "introspectable";
        
        bool Deprecated { get; }
        string? DeprecatedVersion { get; }
        bool Introspectable { get; set; }
    }
}
