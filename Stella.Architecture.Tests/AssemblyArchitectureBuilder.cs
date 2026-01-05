using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Validators;
using System.Reflection;

namespace Stella.Architecture.Tests;

public sealed class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly NamespaceValidator _namespaceValidator = new();
    private readonly AssemblyValidator _assemblyValidator = new();
    private readonly DependencyValidator _dependencyValidator = new();


    private readonly List<TypeArchitectureBuilder> _typeBuilders = [];


    private AssemblyArchitectureBuilder(Assembly assembly)
    {
        _assembly = assembly;
    }

    public static AssemblyArchitectureBuilder ForAssembly(Assembly assembly)
    {
        return new AssemblyArchitectureBuilder(assembly);
    }

    /// <summary>
    /// Validates specific characteristics for types in the assembly that match the target type.
    /// Applies to all types where Type.IsAssignableFrom returns true for the target type.
    /// Configure validation rules using the provided TypeArchitectureBuilder action.
    /// </summary>
    /// <typeparam name="TTarget">The target type to validate</typeparam>
    /// <param name="configure">Configuration action for type validation rules</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithType<TTarget>(Action<TypeArchitectureBuilder> configure)
    {
        return WithType(typeof(TTarget), configure);
    }

    /// <summary>
    /// Validates specific characteristics for types in the assembly that match the target type.
    /// Applies to all types where Type.IsAssignableFrom returns true for the target type.
    /// Configure validation rules using the provided TypeArchitectureBuilder action.
    /// </summary>
    /// <param name="type">The target type to validate</param>
    /// <param name="configure">Configuration action for type validation rules</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithType(Type type, Action<TypeArchitectureBuilder> configure)
    {
        var typeBuilder = TypeArchitectureBuilder.ForType(type);
        configure(typeBuilder);
        _typeBuilders.Add(typeBuilder);
        return this;
    }

    /// <summary>
    /// Validates that the assembly does not depend on any assembly matching the specified regular expression.
    /// Any dependency on a matching assembly will cause validation to fail.
    /// </summary>
    /// <param name="regularExpression">Regular expression pattern applied to referenced assembly full names</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithAssemblyForbiddenDependency(string regularExpression)
    {
        _assemblyValidator.WithAssemblyForbiddenDependency(regularExpression);
        return this;
    }

    /// <summary>
    /// Validates that the namespace is completely isolated with no inbound or outbound dependencies.
    /// Types outside the namespace cannot depend on types inside it, and types inside cannot depend on types outside it.
    /// Any dependency violation will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to isolate (includes child namespaces)</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceIsolated(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);

        return this;
    }

    /// <summary>
    /// Validates that types outside the namespace do not depend on types inside the namespace.
    /// Any type outside the namespace that references a type inside will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to protect from external dependencies (includes child namespaces)</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceNoInboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Validates that types inside the namespace do not depend on types outside the namespace.
    /// Any type inside the namespace that references a type outside will cause validation to fail.
    /// </summary>
    /// <param name="namespaceName">The namespace to restrict outbound dependencies from (includes child namespaces)</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceNoOutboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Validates that only specific types can have dependencies to the target type.
    /// Any other type in the assembly that depends on the target type will cause validation to fail.
    /// </summary>
    /// <typeparam name="TTarget">The type that should have restricted inbound dependencies</typeparam>
    /// <param name="allowedDependentTypes">Types that are allowed to depend on TTarget</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithDependencyUsedOnly<TTarget>(params Type[] allowedDependentTypes)
    {
        return WithDependencyUsedOnly(typeof(TTarget), true, allowedDependentTypes);
    }

    /// <summary>
    /// Validates that only specific types can have dependencies to the target type.
    /// Any other type in the assembly that depends on the target type will cause validation to fail.
    /// </summary>
    /// <param name="targetType">The type that should have restricted inbound dependencies</param>
    /// <param name="allowedDependentTypes">Types that are allowed to depend on targetType</param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithDependencyUsedOnly(Type targetType, bool excludeCompilerGenerated = true, params Type[] allowedDependentTypes)
    {
        _dependencyValidator.WithDependencyUsedOnly(targetType, allowedDependentTypes, excludeCompilerGenerated);
        return this;
    }

    public void ShouldBeValid()
    {
        int maxExceptions = 15;
        var exceptions = new List<Exception>(maxExceptions);

        var allTypes = _assembly.GetTypes();

        exceptions.AddRange(_namespaceValidator.ShouldBeValid(allTypes));
        AssertExceptions(exceptions, maxExceptions);
        exceptions.AddRange(_assemblyValidator.ShouldBeValid(_assembly));
        AssertExceptions(exceptions, maxExceptions);
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