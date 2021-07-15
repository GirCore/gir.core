using System.Runtime.CompilerServices;

namespace cairo
{
    internal partial class Module
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            // InitializeDllImport();
            
            // We override the normal DllImporter generated for us
            // as we need to take into account cairo being spread
            // across multiple shared libraries.
            Native.DllImportOverride.Initialize();
            RegisterTypes();
        }

        static partial void InitializeDllImport();
        static partial void RegisterTypes();
    }
}
