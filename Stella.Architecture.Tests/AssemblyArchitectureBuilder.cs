using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public sealed class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly List<System.Text.RegularExpressions.Regex> _forbiddenAssemblyDependencyRegularExpressions = [];
    private readonly IsolatedNamespaceValidator _isolatedNamespaceValidator;

    private AssemblyArchitectureBuilder(Assembly assembly)
    {
        _assembly = assembly;
        _isolatedNamespaceValidator = new IsolatedNamespaceValidator();
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
    public AssemblyArchitectureBuilder WithForbiddenAssemblyDependency(string regularExpression)
    {
        var regExpression = new System.Text.RegularExpressions.Regex(regularExpression,
            System.Text.RegularExpressions.RegexOptions.Compiled);
        _forbiddenAssemblyDependencyRegularExpressions.Add(regExpression);
        return this;
    }

    /// <summary>
    /// Isolated Namespace without Inbound or Outbound dependencies
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithIsolatedNamespace(string namespaceName)
    {
        _isolatedNamespaceValidator.WithNoInboundDependenciesInNamespace(namespaceName);
        _isolatedNamespaceValidator.WithNoOutboundDependenciesInNamespace(namespaceName);

        return this;
    }

    /// <summary>
    /// All Types outside the Namespace should not depend on types inside the Namespace, in the same assembly
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNoInboundDependenciesInNamespace(string namespaceName)
    {
        _isolatedNamespaceValidator.WithNoInboundDependenciesInNamespace(namespaceName);
        return this;
    }

    /// <summary>
    /// Types in the Namespace should not depend on types outside the Namespace in the same assembly
    /// </summary>
    /// <param name="namespaceName">Validation will be from Namespace to "child namespaces" </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithNoOutboundDependenciesInNamespace(string namespaceName)
    {
        _isolatedNamespaceValidator.WithNoOutboundDependenciesInNamespace(namespaceName);
        return this;
    }

    public void ShouldBeValid()
    {
        var exceptions = new List<Exception>(15);

        var allTypes = _assembly.GetTypes();

        exceptions.AddRange(_isolatedNamespaceValidator.ShouldBeValid(allTypes));
        exceptions.AddRange(ShouldNotDependOnForbiddenAssemblies());


        if (exceptions.Any())
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }

    private IEnumerable<AssertAssembyDependencyException> ShouldNotDependOnForbiddenAssemblies()
    {
        if (!_forbiddenAssemblyDependencyRegularExpressions.Any())
            return [];

        return _assembly.GetReferencedAssemblies()
            .Where(a => _forbiddenAssemblyDependencyRegularExpressions.Any(r => r.IsMatch(a.FullName ?? a.Name)))
            .Select(a => new AssertAssembyDependencyException(a));
    }
}