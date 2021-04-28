using System;
using System.Collections.Generic;
using Generator.Services.Writer;
using Repository;
using Repository.Model;
using Repository.Analysis;
using Scriban.Runtime;

namespace Generator.Factories
{
    public class ScriptObjectFactory
    {
        public ScriptObject CreateBase(Namespace currentNamespace)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import("write_native_arguments", new Func<ParameterList, string>(a => a.WriteNative(currentNamespace)));
            scriptObject.Import("write_native_arguments_no_safehandle", new Func<ParameterList, string>(a => a.WriteNative(currentNamespace, useSafeHandle: false)));
            scriptObject.Import("write_managed_arguments", new Func<ParameterList, string>(a => a.WriteManaged(currentNamespace)));
            scriptObject.Import("write_native_return_value", new Func<ReturnValue, string>(a => a.WriteNative(currentNamespace)));
            scriptObject.Import("write_managed_return_value", new Func<ReturnValue, string>(a => a.WriteManaged(currentNamespace)));
            scriptObject.Import("write_native_method", new Func<Method, string>(m => m.WriteNative(currentNamespace)));
            scriptObject.Import("write_managed_method", new Func<Method, string, string>((m, name) => m.WriteManaged(new SymbolName(name), currentNamespace)));
            scriptObject.Import("get_if", new Func<string, bool, string>(TemplateWriter.GetIf));

            return scriptObject;
        }

        public ScriptObject CreateComplex(Namespace currentNamespace)
        {
            var scriptObject = CreateBase(currentNamespace);
            scriptObject.Import("write_inheritance", new Func<SymbolReference?, IEnumerable<SymbolReference>, string>((s, l) => TemplateWriter.WriteInheritance(s, l, currentNamespace)));
            scriptObject.Import("write_native_parent", new Func<SymbolReference?, string>(s => TemplateWriter.WriteNativeParent(s, currentNamespace)));
            scriptObject.Import("write_native_fields", new Func<IEnumerable<Field>, string>(f => f.WriteNative(currentNamespace)));
            scriptObject.Import("get_signal_data", new Func<Signal, SignalHelper>(s => new SignalHelper(s)));
            scriptObject.Import("write_signal_args_properties", new Func<ParameterList, string>(a => a.WriteSignalArgsProperties(currentNamespace)));
            scriptObject.Import("write_callback_marshaller", new Func<ParameterList, ReturnValue, string>((a, r) => a.WriteCallbackMarshaller(r, currentNamespace)));
            scriptObject.Import("return_value_is_void", new Func<ReturnValue, bool>(r => r.IsVoid()));
            scriptObject.Import("write_struct_fields", new Func<IEnumerable<Field>, string>(f => f.WriteNative(currentNamespace)));
            scriptObject.Import("write_union_fields", new Func<IEnumerable<Field>, string>(f => f.WriteUnionStructFields(currentNamespace)));
            scriptObject.Import("write_native_field_delegates", new Func<IEnumerable<Field>, string>(f => f.WriteNativeDelegates(currentNamespace)));
            return scriptObject;
        }

        public ScriptObject CreateComplexForSymbol(Namespace currentNamespace, Symbol symbol)
        {
            var scriptObject = CreateComplex(currentNamespace);
            scriptObject.Import(symbol);
            //TODO: Workaround as long as scriban indexer are broken see https://github.com/scriban/scriban/issues/333
            scriptObject.Import("get_metadata", new Func<string, object?>(key => symbol.Metadata[key]));
            scriptObject.Import("write_managed_property_descriptor", new Func<Property, string>(p => new Properties.Descriptor(p, symbol, currentNamespace).Write()));
            scriptObject.Import("write_managed_property", new Func<Property, string>(p => new Properties.Definition(p, currentNamespace).Write()));
            return scriptObject;
        }
    }
}
