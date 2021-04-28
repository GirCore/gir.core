using Repository;
using Repository.Model;

namespace Generator.Methods
{
    internal class Constructor : Base
    {
        private readonly Method _method;
        private readonly SymbolName _parentName;
        private readonly Namespace _currentNamespace;

        public Constructor(Method method, SymbolName parentName, Namespace currentNamespace)
        {
            _method = method;
            _parentName = parentName;
            _currentNamespace = currentNamespace;
        }
        
        
        
        public override string Generate()
        {
            throw new System.NotImplementedException();
        }
    }
}
