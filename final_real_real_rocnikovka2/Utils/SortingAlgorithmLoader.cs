using final_real_real_rocnikovka2.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace final_real_real_rocnikovka2.Utils
{
    public class SortingAlgorithmLoader
    {
        private readonly Assembly _assembly;

        public SortingAlgorithmLoader(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public List<SortingAlgorithm> LoadAlgorithms()
        {
            var algorithms = new List<SortingAlgorithm>();
            var algorithmTypes = GetSortingAlgorithmTypes();

            foreach (var type in algorithmTypes)
            {
                var instance = GetInstance(type);
                if (instance != null)
                {
                    algorithms.Add(instance);
                }
            }

            return algorithms;
        }

        private IEnumerable<Type> GetSortingAlgorithmTypes()
        {
            return _assembly.GetTypes()
                            .Where(t => t.IsSubclassOf(typeof(SortingAlgorithm)) && !t.IsAbstract);
        }

        private SortingAlgorithm GetInstance(Type algorithmType)
        {
            var instanceProperty = algorithmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            return instanceProperty?.GetValue(null) as SortingAlgorithm;
        }
    }
}
