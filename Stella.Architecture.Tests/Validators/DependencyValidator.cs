using Stella.Architecture.Tests.Exceptions;
using System.Collections.Immutable;

namespace Stella.Architecture.Tests.Validators;

internal class DependencyValidator
{
    public record TypeDependencies(Type[] Types, bool ExcludeCompilerGenerated);
    private readonly Dictionary<Type, TypeDependencies> _allowedDependentTypes = [];

    public void WithDependencyUsedOnly(Type targetType, Type[] allowedDependentTypes, bool excludeCompilerGenerated)
    {

        _allowedDependentTypes.Add(targetType, new TypeDependencies(allowedDependentTypes, excludeCompilerGenerated));
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
                // Check if we should exclude compiler-generated types
                if (allowedTypes.ExcludeCompilerGenerated && IsCompilerGenerated(currentType))
                    continue;

                if (!allowedTypes.Types.Any(allowed => allowed.IsAssignableFrom(currentType)))
                {
                    yield return new AssertTypeDependencyException(
                        $"Type {currentType} depends on {typeDependency}, which is not valid.", currentType,
                        typeDependency);
                }
            }
        }
    }

    private static bool IsCompilerGenerated(Type type)
    {
        // Check for compiler-generated attribute
        if (type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
            return true;

        // Check for compiler-generated naming patterns
        // Examples: <>c__DisplayClass, <>c, <PrivateImplementationDetails>
        if (type.Name.Contains("<>") || type.Name.Contains("__"))
            return true;

        // Check if it's a nested type of a compiler-generated type
        if (type.IsNested && type.DeclaringType != null && IsCompilerGenerated(type.DeclaringType))
            return true;

        return false;
    }
}