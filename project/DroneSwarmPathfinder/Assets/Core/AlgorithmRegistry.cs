using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DroneSwarmPathfinder.Core.Simulation
{
    /// <summary>
    /// Class that uses reflection to find all loaded pathfinding algorithms
    /// </summary>
    public static class AlgorithmRegistry
    {
        /// <summary>
        /// Loads an external DLL from the hard drive into the applications memory
        /// </summary>
        /// <param name="absoluteFilePath">The absolute path to the .dll file</param>
        /// <returns>True if loaded successfully</returns>
        public static bool LoadExternalAlgorithmDll(string absoluteFilePath)
        {
            try
            {
                Assembly.LoadFrom(absoluteFilePath);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: show this in UI/some other way
                Console.WriteLine($"Failed to load external algorithm: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds and instantiates all classes implementing IPathfindingAlgorithm
        /// </summary>
        public static IEnumerable<IPathfindingAlgorithm> DiscoverAlgorithms()
        {
            var targetType = typeof(IPathfindingAlgorithm);

            // Reflection (+LINQ) to find all matching types across all loaded assemblies
            var algorithmTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => GetTypesSafely(assembly))
                .Where(type => targetType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var type in algorithmTypes)
            {
                // Create an instance of the algorithm (NEED A PARAMETERLESS CONSTRUCTOR!!!)
                yield return (IPathfindingAlgorithm)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Helper to prevent ReflectionTypeLoadException if an assembly has missing dependencies (AI usage, couldn't figure out how to safe-proof it well...)
        /// </summary>
        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}