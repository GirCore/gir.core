using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Repository;
using Repository.Model;

namespace Generator.Methods
{
    internal class Instance : Base
    {
        private readonly Method _method;
        private readonly Namespace _currentNamespace;
        private readonly SymbolName _parent;

        private readonly IEnumerable<Parameter> _nativeParams;
        private readonly IEnumerable<SingleParameter> _managedParams;
        private readonly IEnumerable<SingleParameter> _nullParams;
        private readonly IEnumerable<SingleParameter> _delegateParams;
        private readonly IEnumerable<SingleParameter> _marshalParams;

        private readonly InstanceParameter? _instanceParameter;
        
        // We use a system of 'Blocks' to make sure resources are allocated and
        // deallocated in the correct order. Blocks are nested inside each other,
        // wrapping the inner blocks with a given 'start' and 'end' statement. This
        // enforces a system of FILO (first in, last out), which makes sure that
        // resources are not accidentally deleted while in-use.
        private readonly BlockStack _stack = new();

        public Instance(Method method, SymbolName parent_name, Namespace currentNamespace)
        {
            _method = method;
            _parent = parent_name;
            _currentNamespace = currentNamespace;

            _nativeParams = method.ParameterList.GetParameters();
            _managedParams = method.ParameterList.GetManagedParameters();
            _nullParams = method.ParameterList.SingleParameters.Except(_managedParams);
            _delegateParams = _managedParams.Where(arg => arg.SymbolReference.GetSymbol().GetType() == typeof(Callback));
            _marshalParams = _managedParams.Except(_delegateParams);
            _instanceParameter = method.ParameterList.InstanceParameter;
        }

        public override string Generate()
        {
            if (!CanGenerateMethod())
                return string.Empty;   

            try
            {
                AddMethodSignature();
                AddInterfaceGuard();
                AddDelegateLifecycleManagement();
                AddParameterConversions();
                AddMethodCall();
                AddReturnValue(); // TODO: ReturnValue in wrong order
            
                return _stack.Build();
            }
            catch (Exception e)
            {
                Log.Warning($"Did not generate method '{_parent}.{_method.SymbolName}': {e.Message}");
                return string.Empty;
            }
        }
        
        public bool CanGenerateMethod()
        {
            // We only support a subset of methods at the
            // moment. Determine if we can generate based on
            // the following criteria:
            
            // No static functions (e.g. non class methods)
            if (_instanceParameter == null)
                return false;
            
            // No in/out/ref parameters
            if (_managedParams.Any(param => param.Direction != Direction.Default))
                return false;
            
            // No delegate return values
            if (_method.ReturnValue.SymbolReference.GetSymbol().GetType() == typeof(Callback))
                return false;
            
            // No record parameters
            if (_managedParams.Any(param => param.SymbolReference.GetSymbol().GetType() == typeof(Record)))
                return false;
            
            // No record return values
            if (_method.ReturnValue.SymbolReference.GetSymbol().GetType() == typeof(Record))
                return false;
            
            // No union parameters
            if (_managedParams.Any(param => param.SymbolReference.GetSymbol().GetType() == typeof(Union)))
                return false;
            
            // No union return values
            if (_method.ReturnValue.SymbolReference.GetSymbol().GetType() == typeof(Union))
                return false;
            
            // No GObject array parametersthrow new System.NotImplementedException();
            if (_managedParams.Any(param => 
                param.SymbolReference.GetSymbol().GetType() == typeof(Class) && 
                param.TypeInformation.Array != null))
                return false;
            
            // No GObject array return values
            if (_method.ReturnValue.SymbolReference.GetSymbol().GetType() == typeof(Class) &&
                _method.ReturnValue.TypeInformation.Array != null)
                return false;

            // Go ahead and generate
            return true;
        }
        
        private void AddMethodSignature()
        {
            var returnValue = _method.ReturnValue.WriteManaged(_currentNamespace);
            var parameterList = _method.ParameterList.WriteManaged(_currentNamespace);
            _stack.Nest(new ()
            {
                Start = $"public {returnValue} {_method.SymbolName}({parameterList})\r\n{{",
                End = "}"
            });
        }

        private void AddInterfaceGuard()
        {
            // For each interface-type parameter, add an ArgumentException guard to
            // make sure it really is a GObject. Hopefully we can enforce this with
            // a code analysis plugin or something similar.
            
            // TODO: Implement Interface-Guards
        }

        private void AddDelegateLifecycleManagement()
        {
            // For each delegate-type parameter, we need to create a delegate
            // handling object. This ensures that the delegate and associated
            // resources are kept valid until the delegate is no longer needed
            // by the C-library.
            
            foreach (var dlgParam in _delegateParams)
            {
                Symbol symbol = dlgParam.SymbolReference.GetSymbol();
                var managedType = dlgParam.WriteType(Target.Managed, _currentNamespace);

                var handlerType = dlgParam.CallbackScope switch
                {
                    Scope.Call => $"{managedType}CallHandler",
                    Scope.Async => $"{managedType}AsyncHandler",
                    Scope.Notified => $"{managedType}NotifiedHandler"
                };

                var alloc = $"var {dlgParam.SymbolName}Handler = new {handlerType}({dlgParam.SymbolName});";

                _stack.Nest(new Block()
                {
                    Start = alloc
                });
            }
        }

        private void AddParameterConversions()
        {
            if (_instanceParameter != null)
                AddParameterConversion(_instanceParameter);
            
            foreach (var parameter in _marshalParams)
                AddParameterConversion(parameter);
        }

        private void AddParameterConversion(Parameter parameter)
        {
            // Skip null parameters
            if (_nullParams.Contains(parameter))
                return;
            
            // If we are the instance parameter, marshal variable 'this'
            var fromParamName = parameter == _instanceParameter
                ? "this"
                : parameter.SymbolName;
            
            var expression = Convert.ManagedToNative(
                fromParam: fromParamName,
                symbol: parameter.SymbolReference.GetSymbol(),
                typeInfo: parameter.TypeInformation,
                currentNamespace: _currentNamespace,
                transfer: parameter.Transfer
            );
            
            var alloc = $"{parameter.WriteType(Target.Native, _currentNamespace)} {parameter.SymbolName}Native = {expression};";
            var dealloc = $"// TODO: Free {parameter.SymbolName}Native";
                
            _stack.Nest(new Block()
            {
                Start = alloc,
                End = dealloc
            });
        }

        private void AddMethodCall()
        {
            var call = new StringBuilder();

            if (!_method.ReturnValue.IsVoid())
                call.Append("var result = ");
                
            // TODO: Handle excluded items
            IEnumerable<string> args = _nativeParams.Select(arg =>
            {
                // TODO: Better type handling
                if (_nullParams.Contains(arg))
                    return "IntPtr.Zero";
                    
                // Do something
                Symbol symbol = arg.SymbolReference.GetSymbol();
                var argText = symbol switch
                {
                    Callback => $"{arg.SymbolName}Handler.NativeCallback",
                    _ => $"{arg.SymbolName}Native"
                };

                return argText;
            });

            call.Append($"Native.{_parent}.Instance.Methods.{_method.SymbolName}(");
            call.Append(string.Join(", ", args));
            call.Append(");\n");
                
            _stack.Nest(new Block()
            {
                Start = call.ToString()
            });
        }

        private void AddReturnValue()
        {
            if (_method.ReturnValue.IsVoid())
                return;

            var expression = Convert.NativeToManaged(
                fromParam: "result",
                symbol: _method.ReturnValue.SymbolReference.GetSymbol(),
                typeInfo: _method.ReturnValue.TypeInformation,
                currentNamespace: _currentNamespace,
                transfer: _method.ReturnValue.Transfer
            );
            
            _stack.Nest(new()
            {
                Start = $"return {expression};"
            });
        }
    }
}
