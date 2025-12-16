using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly Dictionary<Type, HashSet<Type>> _typeDependencies = [];
    private readonly List<string> _noExternalDependenciesNamespaces = [];
    private readonly List<string> _isolatedNamespaces = [];


    private AssemblyArchitectureBuilder(Assembly assembly)
    {
        _assembly = assembly;
    }

    public static AssemblyArchitectureBuilder ForAssembly(Assembly assembly)
    {
        return new AssemblyArchitectureBuilder(assembly);
    }

    /// <summary>
    /// Isolated Namespace without Inbound or Outbound dependencies
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithIsolatedNamespace(string namespaceName)
    {
        WithNoInboundDependenciesInNamespace(namespaceName);
        WithNoOutboundDependenciesInNamespace(namespaceName);
        return this;
    }

    /// <summary>
    /// All Types outside the Namespace should not depend on types inside the Namespace, in the same assembly
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNoInboundDependenciesInNamespace(string namespaceName)
    {
        _isolatedNamespaces.Add(namespaceName);
        return this;
    }

    /// <summary>
    /// Types in the Namespace should not depend on types outside the Namespace in the same assembly
    /// </summary>
    /// <param name="namespaceName">Validation will be from Namespace to "child namespaces" </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNoOutboundDependenciesInNamespace(string namespaceName)
    {
        _noExternalDependenciesNamespaces.Add(namespaceName);
        return this;
    }

    public void ShouldBeValid()
    {
        var exceptions = new List<AssertInvalidDependencyException>(15);

        var allTypes = _assembly.GetTypes();

        exceptions.AddRange(ShouldNotDependOnExternalNamespace(allTypes));

        exceptions.AddRange(ShouldNotDependOnIsolatedNamespace(allTypes));

        if (exceptions.Any())
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }

    private IEnumerable<AssertInvalidDependencyException> ShouldNotDependOnIsolatedNamespace(Type[] allTypes)
    {
        foreach (var isolatedNamespace in _isolatedNamespaces)
        {
            var typesOutsideNamespace = allTypes
                .Where(t => t.Namespace is null
                            || (t.Namespace != isolatedNamespace && !t.Namespace.StartsWith(isolatedNamespace + ".")))
                .ToList();

            var exceptionsCount = 0;
            foreach (var type in typesOutsideNamespace)
                foreach (var exception in ShouldNotDependOnIsolatedNamespace(type, isolatedNamespace))
                {
                    yield return exception;
                    exceptionsCount++;
                    if (exceptionsCount > 15)
                        break;
                }
        }
    }
    private List<AssertInvalidDependencyException> ShouldNotDependOnIsolatedNamespace(Type type,
        string isolatedNamespace)
    {
        var exceptions = new List<AssertInvalidDependencyException>();
        var referencedTypes = GetReferencedTypes(type);

        foreach (var referencedType in referencedTypes)
        {
            if (referencedType.Namespace == null)
                continue;

            var isInSameIsolatedNamespace = referencedType.Namespace == isolatedNamespace ||
                                            referencedType.Namespace.StartsWith(isolatedNamespace + ".");

            if (isInSameIsolatedNamespace)
            {
                exceptions.Add(new AssertInvalidDependencyException(
                    $"Type '{type.FullName}' depends on isolated namespace '{isolatedNamespace}' " +
                    $"references type '{referencedType.FullName}' from isolated namespace '{referencedType.Namespace}'",
                    type, referencedType));

                if (exceptions.Count > 3)
                    break;
            }
        }

        return exceptions;
    }
    private IEnumerable<AssertInvalidDependencyException> ShouldNotDependOnExternalNamespace(Type[] allTypes)
    {
        foreach (var isolatedNamespace in _noExternalDependenciesNamespaces)
        {
            var typesInNamespace = allTypes
                .Where(t => t.Namespace != null &&
                            (t.Namespace == isolatedNamespace ||
                             t.Namespace.StartsWith(isolatedNamespace + ".")))
                .ToList();

            var exceptionsCount = 0;
            foreach (var type in typesInNamespace)
                foreach (var exception in ShouldNotDependOnComponentsOutsideNamespace(type, isolatedNamespace))
                {
                    yield return exception;
                    exceptionsCount++;
                    if (exceptionsCount > 15)
                        break;
                }
        }
    }

    private List<AssertInvalidDependencyException> ShouldNotDependOnComponentsOutsideNamespace(Type type,
        string isolatedNamespace)
    {
        var exceptions = new List<AssertInvalidDependencyException>();
        var referencedTypes = GetReferencedTypes(type);

        foreach (var referencedType in referencedTypes)
        {
            if (referencedType.Namespace == null)
                continue;

            var isInSameIsolatedNamespace = referencedType.Namespace == isolatedNamespace ||
                                            referencedType.Namespace.StartsWith(isolatedNamespace + ".");

            if (!isInSameIsolatedNamespace)
            {
                exceptions.Add(new AssertInvalidDependencyException(
                    $"Type '{type.FullName}' in isolated namespace '{isolatedNamespace}' " +
                    $"references type '{referencedType.FullName}' from outside namespace '{referencedType.Namespace}'",
                    type, referencedType));

                if (exceptions.Count > 3)
                    break;
            }
        }

        return exceptions;
    }

    private HashSet<Type> GetReferencedTypes(Type type)
    {
        if (_typeDependencies.TryGetValue(type, out var result))
        {
            return result;
        }
        var referencedTypes = new HashSet<Type>();

        if (type is { BaseType: not null } && type.BaseType != typeof(object) && type.BaseType.Assembly == _assembly)
            referencedTypes.Add(type.BaseType);

        foreach (var @interface in type.GetInterfaces())
            if (@interface.Assembly == _assembly)
                referencedTypes.Add(@interface);

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                             BindingFlags.Static))
            if (field.FieldType.Assembly == _assembly)
                referencedTypes.Add(field.FieldType);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                    BindingFlags.Instance | BindingFlags.Static))
            if (property.PropertyType.Assembly == _assembly)
                referencedTypes.Add(property.PropertyType);

        foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic |
                                                         BindingFlags.Instance | BindingFlags.Static))
            foreach (var parameter in constructor.GetParameters())
                if (parameter.ParameterType.Assembly == _assembly)
                    referencedTypes.Add(parameter.ParameterType);

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (method.ReturnType != null && method.ReturnType.Assembly == _assembly)
                referencedTypes.Add(method.ReturnType);

            foreach (var parameter in method.GetParameters())
                if (parameter.ParameterType.Assembly == _assembly)
                    referencedTypes.Add(parameter.ParameterType);
        }

        _typeDependencies.TryAdd(type, referencedTypes);

        return referencedTypes;
    }
}