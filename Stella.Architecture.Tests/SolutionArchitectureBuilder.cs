using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Validators;
using System.Reflection;

namespace Stella.Architecture.Tests;

/// <summary>
/// Provides a fluent API for configuring and validating architectural rules across multiple assemblies.
/// Enables solution-wide validation of type characteristics, naming conventions, namespace patterns, and cross-assembly dependencies.
/// </summary>
public sealed class SolutionArchitectureBuilder
{
    private readonly List<Assembly> _assemblies;
    private readonly NamespaceValidator _namespaceValidator = new();
    private readonly AssemblyValidator _assemblyValidator = new();
    private readonly DependencyValidator _dependencyValidator = new();
    private readonly List<ITypeArchitectureBuilder> _typeBuilders = [];

    private SolutionArchitectureBuilder(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies.ToList();
    }

    /// <summary>
    /// Creates a new SolutionArchitectureBuilder for the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to validate.</param>
    public static SolutionArchitectureBuilder ForAssemblies(params Assembly[] assemblies)
    {
        return new SolutionArchitectureBuilder(assemblies);
    }

    /// <summary>
    /// Creates a new SolutionArchitectureBuilder for the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to validate.</param>
    public static SolutionArchitectureBuilder ForAssemblies(IEnumerable<Assembly> assemblies)
    {
        return new SolutionArchitectureBuilder(assemblies);
    }

    /// <summary>
    /// Adds an additional assembly to the solution validation scope.
    /// </summary>
    /// <param name="assembly">The assembly to add.</param>
    public SolutionArchitectureBuilder WithAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Validates specific characteristics for types across all assemblies that match the target type.
    /// Applies to all types where Type.IsAssignableFrom returns true for the target type.
    /// Configure validation rules using the provided TypeArchitectureBuilder action.
    /// </summary>
    /// <typeparam name="TTarget">The target type to validate</typeparam>
    /// <param name="configure">Configuration action for type validation rules</param>
    public SolutionArchitectureBuilder WithType<TTarget>(Action<TypeArchitectureBuilder> configure)
    {
        return WithType(typeof(TTarget), configure);
    }

    /// <summary>
    /// Validates specific characteristics for types across all assemblies that match the target type.
    /// Applies to all types where Type.IsAssignableFrom returns true for the target type.
    /// Configure validation rules using the provided TypeArchitectureBuilder action.
    /// </summary>
    /// <param name="type">The target type to validate</param>
    /// <param name="configure">Configuration action for type validation rules</param>
    public SolutionArchitectureBuilder WithType(Type type, Action<TypeArchitectureBuilder> configure)
    {
        var typeBuilder = TypeArchitectureBuilder.ForType(type);
        configure(typeBuilder);
        _typeBuilders.Add(typeBuilder);
        return this;
    }

    /// <summary>
    /// Validates that no assembly in the solution depends on any assembly matching the specified regular expression.
    /// Any dependency on a matching assembly will cause validation to fail.
    /// </summary>
    /// <param name="regularExpression">Regular expression pattern applied to referenced assembly full names</param>
    public SolutionArchitectureBuilder WithAssemblyForbiddenDependency(string regularExpression)
    {
        _assemblyValidator.WithAssemblyForbiddenDependency(regularExpression);
        return this;
    }

    /// <summary>
    /// Validates that the namespace is completely isolated with no inbound or outbound dependencies across all assemblies.
    /// Types outside the namespace cannot depend on types inside it, and types inside cannot depend on types outside it.
    /// Any dependency violation will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to isolate (includes child namespaces)</param>
    public SolutionArchitectureBuilder WithNamespaceIsolated(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Validates that types outside the namespace do not depend on types inside the namespace across all assemblies.
    /// Any type outside the namespace that references a type inside will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to protect from external dependencies (includes child namespaces)</param>
    public SolutionArchitectureBuilder WithNamespaceNoInboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Validates that types inside the namespace do not depend on types outside the namespace across all assemblies.
    /// Any type inside the namespace that references a type outside will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to restrict outbound dependencies from (includes child namespaces)</param>
    public SolutionArchitectureBuilder WithNamespaceNoOutboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Validates that only specific types can have dependencies to the target type across all assemblies.
    /// Any other type in the assemblies that depends on the target type will cause validation to fail.
    /// </summary>
    /// <typeparam name="TTarget">The type that should have restricted inbound dependencies</typeparam>
    /// <param name="allowedDependentTypes">Types that are allowed to depend on TTarget</param>
    public SolutionArchitectureBuilder WithDependencyUsedOnly<TTarget>(params Type[] allowedDependentTypes)
    {
        return WithDependencyUsedOnly(typeof(TTarget), true, allowedDependentTypes);
    }

    /// <summary>
    /// Validates that only specific types can have dependencies to the target type or derived types if target type is an interface across all assemblies.
    /// Any other type in the assemblies that depends on the target type will cause validation to fail.
    /// </summary>
    /// <param name="targetType">The type that should have restricted inbound dependencies</param>
    /// <param name="excludeCompilerGenerated">Exclude Generated code from the compiler</param>
    /// <param name="allowedDependentTypes">Types that are allowed to depend on targetType. Or Derived Types if allowed type is an Interface</param>
    public SolutionArchitectureBuilder WithDependencyUsedOnly(Type targetType, bool excludeCompilerGenerated = true, params Type[] allowedDependentTypes)
    {
        _dependencyValidator.WithDependencyUsedOnly(targetType, allowedDependentTypes, excludeCompilerGenerated);
        return this;
    }

    /// <summary>
    /// Validates all configured architectural rules across all assemblies.
    /// Throws an AssertArchitectureException if any violations are found.
    /// </summary>
    public void ShouldBeValid()
    {
        int maxExceptions = 15;
        var exceptions = new List<Exception>(maxExceptions);

        var allTypes = _assemblies.SelectMany(a => a.GetTypes()).ToArray();

        exceptions.AddRange(_namespaceValidator.ShouldBeValid(allTypes));
        AssertExceptions(exceptions, maxExceptions);

        foreach (var assembly in _assemblies)
        {
            exceptions.AddRange(_assemblyValidator.ShouldBeValid(assembly));
            AssertExceptions(exceptions, maxExceptions);
        }

        exceptions.AddRange(_dependencyValidator.ShouldBeValid(allTypes));
        AssertExceptions(exceptions, maxExceptions);

        foreach (var typeArchitectureBuilder in _typeBuilders)
        {
            exceptions.AddRange(typeArchitectureBuilder.ShouldBeValid(allTypes));
        }

        AssertExceptions(exceptions);
    }

    private static void AssertExceptions(List<Exception> exceptions, int throwIfEqualOrMoreThan = 1)
    {
        if (exceptions.Count >= throwIfEqualOrMoreThan)
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }
}
