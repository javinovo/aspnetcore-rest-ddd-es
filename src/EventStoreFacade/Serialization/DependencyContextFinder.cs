using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventStoreFacade.Serialization
{
    /// <summary>
    /// Dependency "Microsoft.Extensions.DependencyModel": "1.0.*"
    /// </summary>
    public class DependencyContextFinder : ITypeFinder
    {
        readonly string _libraryNamePrefix;
        readonly List<Type> _exportedTypes = new List<Type>();

        /// <summary>
        /// </summary>
        /// <param name="libraryNamePrefix">Optional prefix to filter out libraries</param>
        public DependencyContextFinder(string libraryNamePrefix = null)
        {
            _libraryNamePrefix = libraryNamePrefix;

            Rescan();
        }

        public void Rescan()
        {
            _exportedTypes.Clear();
            _exportedTypes.AddRange(
                DependencyContext.Default.RuntimeLibraries
                .Where(lib => string.IsNullOrWhiteSpace(_libraryNamePrefix) || lib.Name.StartsWith(_libraryNamePrefix))
                .Select(lib => Assembly.Load(new AssemblyName(lib.Name)))
                .SelectMany(x => x.ExportedTypes));
        }

        public Type Find(string typeFullName) =>
            _exportedTypes.SingleOrDefault(t => t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase));
    }
}
