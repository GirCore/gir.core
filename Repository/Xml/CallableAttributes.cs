namespace Repository.Xml
{
    public interface CallableAttributes : InfoAttributes
    {
        const string NameAttribute = "name";
        const string IdentifierAttribute = "identifier";
        const string IdentifierNamespace = "http://www.gtk.org/introspection/c/1.0";
        const string ThrowsAttribute = "throws";
        const string MovedToAttribute = "moved-to";
        
        string? Name { get; }
        string? Identifier { get; }
        bool Throws { get; }
        string? MovedTo { get; }
    }
}
