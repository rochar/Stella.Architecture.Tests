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
        if (currentType.FullName.IndexOf("InvalidDerivedIDependencyDependant") >= 0)
        {
        }

        foreach (var typeDependency in typeDependencies)
            if (GetAllowedDependencyTypes(typeDependency, out var allowedTypes))
            {
                if (allowedTypes.ExcludeCompilerGenerated && IsCompilerGenerated(currentType))
                    continue;

                //Support only derived types from interfaces, concrete base type are not considered avoiding being very
                //strict and allow the clients to define the rules base on concrete implementations
                if (!allowedTypes.Types.Any(allowed =>
                        currentType == allowed || (allowed.IsInterface && allowed.IsAssignableFrom(currentType))))
                    yield return new AssertTypeDependencyException(
                        $"Type {currentType} depends on {typeDependency}, which is not valid.", currentType,
                        typeDependency);
            }
    }

    private bool GetAllowedDependencyTypes(Type typeDependency, out TypeDependencies allowedTypes)
    {
        if (!typeDependency.IsInterface)
            return _allowedDependentTypes.TryGetValue(typeDependency, out allowedTypes);

        //include derived types
        var includeTypes = new List<Type>();
        var excludeCompilerGenerated = false;
        foreach (var entry in _allowedDependentTypes)
            if (typeDependency.IsAssignableFrom(entry.Key))
            {
                includeTypes.AddRange(entry.Value.Types);
                excludeCompilerGenerated = excludeCompilerGenerated || entry.Value.ExcludeCompilerGenerated;
            }

        allowedTypes = new TypeDependencies(includeTypes.ToArray(), excludeCompilerGenerated);

        return includeTypes.Any();
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

        // Check if it's a nested type of compiler-generated type
        if (type is { IsNested: true, DeclaringType: not null } && IsCompilerGenerated(type.DeclaringType))
            return true;

        return false;
    }
}