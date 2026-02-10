using Stella.Architecture.Tests.Exceptions;
using System.Collections.Immutable;

namespace Stella.Architecture.Tests.Validators;

internal class NamespaceValidator
{
    private readonly HashSet<string> _noInboundDependenciesInNamespace = [];
    private readonly List<NamespaceDependenciesConfig> _noOutboundDependenciesInNamespace = [];


    private record NamespaceDependenciesConfig(string Namespace, ImmutableHashSet<string> ExceptionsNamespaces);

    public void WithNamespaceNoInboundDependencies(string ns)
    {
        _noInboundDependenciesInNamespace.Add(ns);
    }

    public void WithNamespaceNoOutboundDependencies(string ns, string[]? exceptToNamespace = null)
    {
        var exceptions = exceptToNamespace ?? [];
        _noOutboundDependenciesInNamespace.Add(new NamespaceDependenciesConfig(ns,
            exceptions.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase)));
    }

    public IEnumerable<AssertTypeDependencyException> ShouldBeValid(Type[] allTypes)
    {
        return ShouldNotHaveInBoundDependencies(allTypes).Concat(ShouldNotHaveOutBoundDependencies(allTypes));
    }

    public IEnumerable<AssertTypeDependencyException> ShouldNotHaveInBoundDependencies(Type[] allTypes)
    {
        foreach (var isolatedNamespace in _noInboundDependenciesInNamespace)
        {
            var typesOutsideNamespace = allTypes
                .Where(t => t.Namespace is null
                            || (t.Namespace != isolatedNamespace && !t.Namespace.StartsWith(isolatedNamespace + ".")))
                .ToList();

            foreach (var type in typesOutsideNamespace)
                foreach (var exception in ShouldNotHaveInBoundDependencies(type, isolatedNamespace))
                    yield return exception;
        }
    }

    private List<AssertTypeDependencyException> ShouldNotHaveInBoundDependencies(Type type,
        string isolatedNamespace)
    {
        var exceptions = new List<AssertTypeDependencyException>();
        var referencedTypes = TypeDependenciesCache.GetInternalReferenceTypes(type);

        foreach (var referencedType in referencedTypes)
        {
            if (referencedType.Namespace == null)
                continue;

            var isInSameIsolatedNamespace = referencedType.Namespace == isolatedNamespace ||
                                            referencedType.Namespace.StartsWith(isolatedNamespace + ".");

            if (isInSameIsolatedNamespace)
                exceptions.Add(new AssertTypeDependencyException(
                    $"Type '{type.FullName}' depends on isolated namespace '{isolatedNamespace}' " +
                    $"references type '{referencedType.FullName}' from isolated namespace '{referencedType.Namespace}'",
                    type, referencedType));
        }

        return exceptions;
    }

    public IEnumerable<AssertTypeDependencyException> ShouldNotHaveOutBoundDependencies(Type[] allTypes)
    {
        foreach (var isolatedNamespace in _noOutboundDependenciesInNamespace)
        {
            var typesInNamespace = allTypes
                .Where(t => t.Namespace != null &&
                            (t.Namespace == isolatedNamespace.Namespace ||
                             t.Namespace.StartsWith(isolatedNamespace.Namespace + ".")))
                .ToList();

            foreach (var type in typesInNamespace)
                foreach (var exception in ShouldNotDependOnComponentsOutsideNamespace(type, isolatedNamespace.Namespace,
                             isolatedNamespace.ExceptionsNamespaces))
                    yield return exception;
        }
    }


    private List<AssertTypeDependencyException> ShouldNotDependOnComponentsOutsideNamespace(Type type,
        string isolatedNamespace, ImmutableHashSet<string> exceptionsNamespaces)
    {
        var exceptions = new List<AssertTypeDependencyException>();
        var referencedTypes = TypeDependenciesCache.GetInternalReferenceTypes(type);

        foreach (var referencedType in referencedTypes)
        {
            if (referencedType.Namespace == null)
                continue;

            var isInSameIsolatedNamespace = referencedType.Namespace == isolatedNamespace
                                            || referencedType.Namespace.StartsWith(isolatedNamespace + ".")
                                            || exceptionsNamespaces.Contains(referencedType.Namespace);

            if (!isInSameIsolatedNamespace)
                exceptions.Add(new AssertTypeDependencyException(
                    $"Type '{type.FullName}' in isolated namespace '{isolatedNamespace}' " +
                    $"references type '{referencedType.FullName}' from outside namespace '{referencedType.Namespace}'",
                    type, referencedType));
        }

        return exceptions;
    }
}