using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Generator.Factories;
using GirLoader.Output.Model;
using Scriban.Runtime;
using Type = GirLoader.Output.Model.Type;

namespace Generator.Services.Writer
{
    internal class WriteModuleService
    {
        private readonly WriteHelperService _writeHelperService;
        private readonly DllImportResolverFactory _dllImportResolverFactory;

        public WriteModuleService(WriteHelperService writeHelperService, DllImportResolverFactory dllImportResolverFactory)
        {
            _writeHelperService = writeHelperService;
            _dllImportResolverFactory = dllImportResolverFactory;
        }

        public void Write(Namespace ns, string outputDir)
        {
            if (ns.SharedLibrary is null)
                throw new Exception($"Namespace {ns.Name} does not provide a shared libraryinfo");

            try
            {
                WriteTypeDictionaryInitialization(ns, outputDir);
                WriteDllImport(ns, outputDir);
            }
            catch (Exception ex)
            {
                Log.Error($"Could not write module for {ns.Name}: {ex.Message}");
            }
        }

        private void WriteTypeDictionaryInitialization(Namespace ns, string outputDir)
        {
            IEnumerable<Type> classes = ns.Classes.Where(x => !x.IsFundamental);
            IEnumerable<Type> records = ns.Records.Where(x => x.GetTypeFunction is not null);
            IEnumerable<Type> unions = ns.Unions.Where(x => x.GetTypeFunction is not null);
            
            IEnumerable<Type> types = classes.Concat(records);

            if (!types.Any())
                return;

            var scriptObject = new ScriptObject()
            {
                { "namespace", ns },
                { "classes", classes },
                { "records", records },
                { "unions", unions }
            };
            scriptObject.Import("write_type_registration", new Func<Type, string>(s => s.WriteTypeRegistrationClass()));
            scriptObject.Import("write_type_registration_record", new Func<Type, string>(s => s.WriteTypeRegistrationBoxed()));
            scriptObject.Import("write_type_registration_union", new Func<Type, string>(s => s.WriteTypeRegistrationBoxed()));

            _writeHelperService.Write(
                projectName: ns.ToCanonicalName(),
                templateName: "module_type_registration.sbntxt",
                folder: Folder.Managed.Classes,
                outputDir: outputDir,
                fileName: "Module.TypeRegistration",
                scriptObject: scriptObject
            );
        }

        private void WriteDllImport(Namespace ns, string outputDir)
        {
            Debug.Assert(
                condition: ns.SharedLibrary is not null,
                message: "sharedLibrary is not set"
            );

            DllImportResolver dllImportResolver = _dllImportResolverFactory.Create(
                sharedLibrary: ns.SharedLibrary,
                namespaceName: ns.Name
            );

            var scriptObject = new ScriptObject
            {
                { "namespace", ns},
                { "windows_dll", dllImportResolver.GetWindowsDllImport() },
                { "linux_dll", dllImportResolver.GetLinuxDllImport() },
                { "osx_dll", dllImportResolver.GetOsxDllImport() }
            };

            _writeHelperService.Write(
                projectName: ns.ToCanonicalName(),
                templateName: "native.dll_import.sbntxt",
                folder: Folder.Managed.Classes,
                outputDir: outputDir,
                fileName: "DllImport",
                scriptObject: scriptObject
            );

            _writeHelperService.Write(
                projectName: ns.ToCanonicalName(),
                templateName: "module_dll_import.sbntxt",
                folder: Folder.Managed.Classes,
                outputDir: outputDir,
                fileName: "Module.DllImport",
                scriptObject: scriptObject
            );
        }
    }
}
