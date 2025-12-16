using System.Reflection;

namespace Stella.Architecture.Tests;

public class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly List<string> _isolatedNamespaces = new();

    private AssemblyArchitectureBuilder(Assembly assembly)
    {
        _assembly = assembly;
    }

    public static AssemblyArchitectureBuilder ForAssembly(Assembly assembly)
    {
        return new AssemblyArchitectureBuilder(assembly);
    }

    /// <summary>
    /// Types in the Namespace should not depend on types outside the Namespace in the same assembly
    /// </summary>
    /// <param name="namespaceName">Validation will be from Namespace to "child namespaces" </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithIsolatedNamespace(string namespaceName)
    {
        _isolatedNamespaces.Add(namespaceName);
        return this;
    }

    public AssemblyArchitectureBuilder WithIsolatedNamespaces(params string[] namespaceNames)
    {
        _isolatedNamespaces.AddRange(namespaceNames);
        return this;
    }

    public void ShouldBeValid()
    {
        var exceptions = new List<AssertInvalidDependencyException>(15);

        var allTypes = _assembly.GetTypes();

        foreach (var isolatedNamespace in _isolatedNamespaces)
        {
            var typesInNamespace = allTypes
                .Where(t => t.Namespace != null &&
                            (t.Namespace == isolatedNamespace ||
                             t.Namespace.StartsWith(isolatedNamespace + ".")))
                .ToList();

            foreach (var type in typesInNamespace)
            {
                exceptions.AddRange(ShouldNotDependOnComponentsOutsideNamespace(type, isolatedNamespace));
                if (exceptions.Count > 15)
                    break;
            }
        }

        if (exceptions.Any())
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
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
                    $"references type '{referencedType.FullName}' from outside namespace '{referencedType.Namespace}'", type, referencedType));

                if (exceptions.Count > 3)
                    break;
            }
        }

        return exceptions;
    }

    private IEnumerable<Type> GetReferencedTypes(Type type)
    {
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
        {
            foreach (var parameter in constructor.GetParameters())
                if (parameter.ParameterType.Assembly == _assembly)
                    referencedTypes.Add(parameter.ParameterType);
        }

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (method.ReturnType != null && method.ReturnType.Assembly == _assembly)
                referencedTypes.Add(method.ReturnType);

            foreach (var parameter in method.GetParameters())
                if (parameter.ParameterType.Assembly == _assembly)
                    referencedTypes.Add(parameter.ParameterType);
        }

        return referencedTypes;
    }
}