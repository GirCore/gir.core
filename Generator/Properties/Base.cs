using Repository.Model;

namespace Generator.Properties
{
    internal class Base
    {
        protected string CommentIfUnsupported(string str, Repository.Model.Property property)
        {
            //TODO: Remove this method if all cases are supported
            return property switch
            {
                {SymbolReference: { Symbol: { } and String }} => str,
                {SymbolReference: { Symbol: { } and PrimitiveValueType }}  => str,
                {SymbolReference: { Symbol: { } and Enumeration }}  => str,
                {SymbolReference: {Symbol: {} and Class}} => str,
                _ => "/*" + str + "*/"
            };
        }
    }
}
