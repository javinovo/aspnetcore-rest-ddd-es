using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace EventStoreFacade.Serialization
{
    /// <summary>
    /// Dependency: "System.Runtime.Loader": "4.0.*"
    /// </summary>
    public class RuntimeLoaderFinder : ITypeFinder
    {
        readonly DirectoryInfo _typesDirectory;
        readonly string _assemblyNamePrefix;
        readonly List<Type> _exportedTypes = new List<Type>();

        /// <summary>
        /// </summary>
        /// <param name="typesDirectory">Directory contaning the assemblies (.dll files) where the event types are defined</param>
        /// <param name="assemblyNamePrefix">Optional name prefix used to filter the assembly files</param>
        public RuntimeLoaderFinder(DirectoryInfo typesDirectory, string assemblyNamePrefix = "")
        {
            _assemblyNamePrefix = assemblyNamePrefix;
            _typesDirectory = typesDirectory;

            Rescan();
        }

        public void Rescan()
        {
            _exportedTypes.Clear();
            _exportedTypes.AddRange(
                _typesDirectory.EnumerateFiles($"{_assemblyNamePrefix}*.dll")
                    .SelectMany(fi => 
                        AssemblyLoadContext.Default.LoadFromAssemblyPath(fi.FullName).ExportedTypes));
        }

        public Type Find(string typeFullName) =>
            _exportedTypes.SingleOrDefault(t => t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase));
    }
}
