using System.Reflection;
using Stella.Architecture.Tests.Validators;

namespace Stella.Architecture.Tests;

internal sealed class AssemblyInSolutionArchitectureBuilder : IAssemblyInSolutionArchitectureBuilder
{
    private readonly Assembly _assembly;
    private readonly AssemblyValidator _solutionAssemblyValidator;

    public AssemblyInSolutionArchitectureBuilder(Assembly assembly, AssemblyArchitectureBuilder assemblyBuilder, IEnumerable<Assembly> solutionAssemblies)
    {
        _assembly = assembly;
        AssemblyBuilder = assemblyBuilder;
        _solutionAssemblyValidator = new AssemblyValidator(assembly);
        _solutionAssemblyValidator.WithSolutionContext(solutionAssemblies);
    }

    public AssemblyArchitectureBuilder AssemblyBuilder { get; }

    public IAssemblyInSolutionArchitectureBuilder WithAllowedSolutionDependencies(params string[] allowedAssemblyNames)
    {
        _solutionAssemblyValidator.WithAllowedSolutionDependencies(allowedAssemblyNames);
        return this;
    }

    public IAssemblyInSolutionArchitectureBuilder WithForbiddenSolutionDependencies(params string[] forbiddenAssemblyNames)
    {
        _solutionAssemblyValidator.WithForbiddenSolutionDependencies(forbiddenAssemblyNames);
        return this;
    }

    public void ShouldBeValid()
    {
        // First validate the standalone rules
        AssemblyBuilder.ShouldBeValid();

        // Then validate solution-specific rules
        var exceptions = _solutionAssemblyValidator.ShouldBeValid().ToList();
        if (exceptions.Any())
        {
            throw new Exceptions.AssertArchitectureException("Invalid Architecture in Solution Context", exceptions.ToArray());
        }
    }
}