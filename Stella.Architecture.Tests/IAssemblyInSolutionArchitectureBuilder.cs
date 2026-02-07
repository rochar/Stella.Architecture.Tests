using System.Reflection;

namespace Stella.Architecture.Tests;

public interface IAssemblyInSolutionArchitectureBuilder
{
    /// <summary>
    /// Exposes the underlying AssemblyArchitectureBuilder for general rule configuration.
    /// </summary>
    AssemblyArchitectureBuilder AssemblyBuilder { get; }

    /// <summary>
    /// Validates that the assembly only depends on the specified solution assemblies.
    /// Any dependency on a solution assembly NOT in this list will cause validation to fail.
    /// </summary>
    /// <param name="allowedAssemblyNames">The names of the solution assemblies that are allowed dependencies</param>
    /// <returns></returns>
    IAssemblyInSolutionArchitectureBuilder WithAllowedSolutionDependencies(params string[] allowedAssemblyNames);

    /// <summary>
    /// Validates that the assembly does NOT depend on the specified solution assemblies.
    /// Any dependency on a solution assembly in this list will cause validation to fail.
    /// </summary>
    /// <param name="forbiddenAssemblyNames">The names of the solution assemblies that are forbidden dependencies</param>
    /// <returns></returns>
    IAssemblyInSolutionArchitectureBuilder WithForbiddenSolutionDependencies(params string[] forbiddenAssemblyNames);
    
    /// <summary>
    /// Validates all configured architectural rules, including solution-specific ones.
    /// </summary>
    void ShouldBeValid();
}