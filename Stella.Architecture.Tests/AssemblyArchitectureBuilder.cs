using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public sealed class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly NamespaceValidator _namespaceValidator = new();
    private readonly AssemblyValidator _assemblyValidator = new();


    private AssemblyArchitectureBuilder(Assembly assembly)
    {
        _assembly = assembly;
    }

    public static AssemblyArchitectureBuilder ForAssembly(Assembly assembly)
    {
        return new AssemblyArchitectureBuilder(assembly);
    }

    /// <summary>
    /// Invalid if Assembly depends on any Assembly that the Full Name matches the regular expression
    /// </summary>
    /// <param name="regularExpression">regular expression applied to dependant Assembly full name </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithAssemblyForbiddenDependency(string regularExpression)
    {
        _assemblyValidator.WithAssemblyForbiddenDependency(regularExpression);
        return this;
    }

    /// <summary>
    /// Isolated Namespace without Inbound or Outbound dependencies
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceIsolated(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);

        return this;
    }

    /// <summary>
    /// All Types outside the Namespace should not depend on types inside the Namespace, in the same assembly
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceNoInboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoInboundDependencies(namespaceName);
        return this;
    }

    /// <summary>
    /// Types in the Namespace should not depend on types outside the Namespace in the same assembly
    /// </summary>
    /// <param name="namespaceName">Validation will be from Namespace to "child namespaces" </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNamespaceNoOutboundDependencies(string namespaceName)
    {
        _namespaceValidator.WithNamespaceNoOutboundDependencies(namespaceName);
        return this;
    }

    public void ShouldBeValid()
    {
        var exceptions = new List<Exception>(15);

        var allTypes = _assembly.GetTypes();

        exceptions.AddRange(_namespaceValidator.ShouldBeValid(allTypes));
        exceptions.AddRange(_assemblyValidator.ShouldBeValid(_assembly));


        if (exceptions.Any())
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }


}