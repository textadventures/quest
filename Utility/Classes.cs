using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TextAdventures.Utility
{
    public static class Classes
    {
        public static bool IsConcrete(Type t)
        {
            return !t.IsAbstract && !t.IsGenericTypeDefinition && !t.IsInterface;
        }

        public static IEnumerable<Type> GetImplementations(Assembly assembly, Type interfaceType)
        {
            return assembly.GetTypes()
                .Where(p => interfaceType.IsAssignableFrom(p))
                .Where(p => IsConcrete(p));
        }

    }
}
