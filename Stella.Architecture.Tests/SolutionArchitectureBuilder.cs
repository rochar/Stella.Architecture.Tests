using System.Reflection;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Validators;

namespace Stella.Architecture.Tests;

/// <summary>
///     Provides a fluent API for configuring and validating architectural rules across multiple assemblies.
///     Enables solution-wide validation of type characteristics, naming conventions, namespace patterns, and
///     cross-assembly dependencies.
/// </summary>
public sealed class SolutionArchitectureBuilder
{
    private readonly List<Assembly> _assemblies;
    private readonly List<IAssemblyInSolutionArchitectureBuilder> _assemblyBuilders = [];
  
    private SolutionArchitectureBuilder(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies.ToList();
    }

    /// <summary>
    ///     Creates a new SolutionArchitectureBuilder for the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to validate.</param>
    public static SolutionArchitectureBuilder ForAssemblies(params Assembly[] assemblies)
    {
        return new SolutionArchitectureBuilder(assemblies);
    }

    /// <summary>
    ///     Creates a new SolutionArchitectureBuilder for the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to validate.</param>
    public static SolutionArchitectureBuilder ForAssemblies(IEnumerable<Assembly> assemblies)
    {
        return new SolutionArchitectureBuilder(assemblies);
    }

    /// <summary>
    ///     Ensures the assembly is included in the solution validation scope.
    ///     The assembly must have been provided in the initial ForAssemblies call.
    /// </summary>
    /// <param name="assembly">The assembly to verify.</param>
    public SolutionArchitectureBuilder WithAssembly(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            throw new ArgumentException($"Assembly {assembly.GetName().Name} was not provided in ForAssemblies", nameof(assembly));
        }
        return this;
    }

    /// <summary>
    ///     Adds an additional assembly to the solution validation scope and allows configuring it.
    /// </summary>
    /// <param name="assembly">The assembly to add.</param>
    /// <param name="configure">Configuration action for the assembly validation rules.</param>
    public SolutionArchitectureBuilder WithAssembly(Assembly assembly, Action<IAssemblyInSolutionArchitectureBuilder> configure)
    {
        if (!_assemblies.Contains(assembly))
        {
            throw new ArgumentException($"Assembly {assembly.GetName().Name} was not provided in ForAssemblies", nameof(assembly));
        }
        
        var builder = new AssemblyInSolutionArchitectureBuilder(assembly, AssemblyArchitectureBuilder.ForAssembly(assembly), _assemblies);
        configure(builder);
        _assemblyBuilders.Add(builder);
        return this;
    }


    /// <summary>
    ///     Validates all configured architectural rules across all assemblies.
    ///     Throws an AssertArchitectureException if any violations are found.
    /// </summary>
    public void ShouldBeValid()
    {
        int maxExceptions = 15;
        var exceptions = new List<Exception>(maxExceptions);
        

        foreach (var builder in _assemblyBuilders)
        {
            try
            {
                builder.ShouldBeValid();
            }
            catch (AssertArchitectureException ex)
            {
                exceptions.AddRange(ex.AssertExceptions);
            }
            AssertExceptions(exceptions, maxExceptions);
        }

        AssertExceptions(exceptions);
    }

    private static void AssertExceptions(List<Exception> exceptions, int throwIfEqualOrMoreThan = 1)
    {
        if (exceptions.Count >= throwIfEqualOrMoreThan)
            throw new AssertArchitectureException("Invalid Architecture", exceptions.ToArray());
    }
}