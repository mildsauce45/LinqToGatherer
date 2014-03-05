using System;
using System.Linq;
using System.Reflection;

namespace LinqToGatherer
{
    internal static class ReflectionExtensions
    {
        internal static bool Implements(this Type concreteType, Type interfaceType)
        {
            if (!interfaceType.IsGenericTypeDefinition)
                return interfaceType.IsAssignableFrom(concreteType);

            var baseType = concreteType;

            while (baseType != null && baseType != typeof(object))
            {
                if (baseType == interfaceType || (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == interfaceType))
                    return true;

                if (baseType.GetInterfaces().Any(i => (i.IsGenericType ? i.GetGenericTypeDefinition() : i) == interfaceType))
                    return true;

                baseType = baseType.BaseType;
            }

            return false;
        }

        internal static Type GetUnderlyingType(this Type t, int index = 0)
        {
            if (t == null)
                return null;

            return !t.IsGenericType ? t : t.GetGenericArguments()[index];
        }

        internal static T GetCustomAttribute<T>(this MemberInfo mi) where T : Attribute
        {
            var attrType = typeof(T);

            if (!typeof(Attribute).IsAssignableFrom(attrType))
                throw new ArgumentException(string.Format("The type {0} does not inherit from Attribute", attrType.Name));

            var attrs = mi.GetCustomAttributes(attrType, false);

            if (attrs.IsNullOrEmpty())
                return null;

            if (attrs.Count() > 1)
                throw new Exception(string.Format("There is more than one attribute of the type {0} on the given method.", attrType.Name));

            return attrs.OfType<T>().First();
        }
    }
}
