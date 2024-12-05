using System.Reflection;

namespace HrCommonApi.Utils;

public static class ReflectionUtils
{
    #region This is the bad code, very naughty
    /// <summary>
    /// This bit of code is to blame for a lot of lazy ass things we can do in this project.
    /// 
    /// So yeah, if you touch this in any way, expect a faulty countdown till the explosion. Teehee.
    /// </summary>
    public static Type[] GetTypesInNamespaceImplementing<TObjectType>(Assembly assembly, string targetNamespace)
    {
        return assembly.GetTypes().Where(type =>
        {
            bool inNamespace = string.Equals(type.Namespace, targetNamespace, StringComparison.Ordinal);
            bool isOfInterface = type.IsAssignableTo(typeof(TObjectType));
            bool isOfSubclass = typeof(TObjectType).IsSubclassOf(type);

            bool isOfBaseClass = false;

            var baseType = type.BaseType;
            if (baseType != null && baseType.IsGenericType)
            {
                baseType = baseType.GetGenericTypeDefinition();
                var matchType = typeof(TObjectType);
                if (matchType.IsGenericType)
                    matchType = matchType.GetGenericTypeDefinition();

                isOfBaseClass = baseType == matchType;
            }

            return inNamespace && (isOfInterface || isOfSubclass || isOfBaseClass);
        }).ToArray();
    }

    /// <summary>
    /// Makes a valiant effort to get the type for the interface. (Absolute fucking guesswork :D)
    /// </summary>
    public static Type? TryGetInterfaceForType(Type type)
    {
        return type.GetInterfaces().FirstOrDefault(t => t.Name.EndsWith(type.Name));
    }
    #endregion
}
