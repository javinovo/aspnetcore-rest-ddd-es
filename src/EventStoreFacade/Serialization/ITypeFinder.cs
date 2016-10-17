using System;

namespace EventStoreFacade.Serialization
{
    public interface ITypeFinder
    {
        /// <summary>
        /// Finds a type by full name (namespace + type name), returning null if not found
        /// </summary>
        Type Find(string typeFullName);

        /// <summary>
        /// Refreshes the available types by rescanning the assemblies
        /// </summary>
        void Rescan();
    }
}
