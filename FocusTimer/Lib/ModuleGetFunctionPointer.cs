using SQLitePCL;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FocusTimer.Lib
{
    public class ModuleGetFunctionPointer : IGetFunctionPointer
    {
        private readonly ProcessModule _module;

        public static ProcessModule GetModule(string moduleName)
        {
            var modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(e => Path.GetFileNameWithoutExtension(e.ModuleName) == moduleName).ToList();
            if (modules.Count == 0)
            {
                throw new ArgumentException($"Found no modules named '{moduleName}' in the current process.", nameof(moduleName));
            }
            if (modules.Count > 1)
            {
                throw new ArgumentException($"Found several modules named '{moduleName}' in the current process.", nameof(moduleName));
            }
            return modules[0];
        }

        public ModuleGetFunctionPointer(string moduleName) : this(GetModule(moduleName))
        {
        }

        public ModuleGetFunctionPointer(ProcessModule module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public IntPtr GetFunctionPointer(string name) => NativeLibrary.TryGetExport(_module.BaseAddress, name, out var address) ? address : IntPtr.Zero;
    }
}
