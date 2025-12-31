using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public sealed class AssemblyArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly List<System.Text.RegularExpressions.Regex> _forbiddenDependenciesRegularExpression = [];
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
    /// Forbidden Dependency
    /// </summary>
    /// <param name="regularExpression">regular expression applied to depency type full name </param>
    /// <returns></returns>
    public AssemblyArchitectureBuilder WithForbiddenDependency(string regularExpression)
    {
        var regExpression = new System.Text.RegularExpressions.Regex(regularExpression, System.Text.RegularExpressions.RegexOptions.Compiled);
        _forbiddenDependenciesRegularExpression.Add(regExpression);
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
        var exceptions = new List<AssertInvalidDependencyException>(15);

        var allTypes = _assembly.GetTypes();

        exceptions.AddRange(_isolatedNamespaceValidator.ShouldBeValid(allTypes));
        exceptions.AddRange(ShouldNotDependOnForbiddenDependencies(allTypes));

        if (exceptions.Any())
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }

    private IEnumerable<AssertInvalidDependencyException> ShouldNotDependOnForbiddenDependencies(Type[] allTypes)
    {
        foreach (var type in allTypes)
        {
            var referencedTypes = TypeDependenciesCache.GetExternalReferenceTypes(type);
            var exceptionsCount = 0;


            foreach (var referencedType in referencedTypes)
            {
                var fullName = referencedType.FullName ?? referencedType.Name;
                foreach (var regex in _forbiddenDependenciesRegularExpression)

                    if (regex.IsMatch(fullName))
                    {
                        exceptionsCount++;
                        yield return new AssertInvalidDependencyException(
                            $"Type '{type.FullName}' depends on forbidden type '{fullName}' (matched by regex '{regex}')",
                            type, referencedType);
                        if (exceptionsCount > 3)
                            yield break;
                    }
            }
        }
    }
}