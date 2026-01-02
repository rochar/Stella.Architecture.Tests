using Stella.Architecture.Tests.Exceptions;
using System.Collections.Immutable;

namespace Stella.Architecture.Tests.Validators;

internal class DependencyValidator
{
    private readonly Dictionary<Type, Type[]> _allowedDependentTypes = [];

    public void WithDependencyUsedOnly(Type targetType, Type[] allowedDependentTypes)
    {
        _allowedDependentTypes.Add(targetType, allowedDependentTypes);
    }

    public IEnumerable<AssertTypeDependencyException> ShouldBeValid(Type[] allTypes)
    {
        foreach (var type in allTypes)
        {
            if (type.IsInterface)
                continue;

            foreach (var exception in ShouldBeValid(type, TypeDependenciesCache.GetExternalReferenceTypes(type)))
                yield return exception;

            foreach (var exception in ShouldBeValid(type, TypeDependenciesCache.GetInternalReferenceTypes(type)))
                yield return exception;
        }
    }

    private IEnumerable<AssertTypeDependencyException> ShouldBeValid(Type currentType,
        ImmutableHashSet<Type> typeDependencies)
    {
        foreach (var typeDependency in typeDependencies)
        {
            if (_allowedDependentTypes.TryGetValue(typeDependency, out var allowedTypes))
            {
                if (!allowedTypes.Any(allowed => allowed.IsAssignableFrom(currentType)))
                {
                    yield return new AssertTypeDependencyException(
                        $"Type {currentType} depends on {typeDependency}, which is not valid.", currentType,
                        typeDependency);
                }
            }
        }
    }
}