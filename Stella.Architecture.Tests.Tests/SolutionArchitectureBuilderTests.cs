using System.Reflection;
using Shouldly;
using Stella.Architecture.Tests.Exceptions;

namespace Stella.Architecture.Tests.Tests;

public class SolutionArchitectureBuilderTests
{
    [Fact]
    public void ShouldBeValidWhenNoRulesAreDefined()
    {
        SolutionArchitectureBuilder.ForAssemblies(Assembly.GetExecutingAssembly())
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldAllowConfiguringAssemblyArchitectureBuilder()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies(executingAssembly)
                .WithAssembly(executingAssembly, builder =>
                {
                    builder.AssemblyBuilder.WithAssemblyForbiddenDependency("Newtonsoft");
                })
                .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ShouldThrowIfAssemblyNotProvidedInForAssemblies()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var otherAssembly = typeof(string).Assembly;

        Should.Throw<ArgumentException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies(executingAssembly)
                .WithAssembly(otherAssembly, _ => { });
        }).Message.ShouldContain("was not provided in ForAssemblies");
    }

    [Fact]
    public void ShouldBeInvalidWhenAssemblyDependsOnNonAllowedSolutionAssembly()
    {
        // For this test, we need at least two assemblies in the solution.
        // We can use the current assembly and another one it depends on, or a dummy assembly.
        // The executing assembly depends on Stella.Architecture.Tests (the main project).
        
        var mainAssembly = typeof(SolutionArchitectureBuilder).Assembly;
        var testAssembly = Assembly.GetExecutingAssembly();

        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies(mainAssembly, testAssembly)
                .WithAssembly(testAssembly, builder =>
                {
                    // Restrict allowed dependencies to NOTHING (within the solution)
                    builder.WithAllowedSolutionDependencies("SomeOtherAssembly");
                })
                .ShouldBeValid();
        });

        exception.AssertExceptions.OfType<AssertAssembyDependencyException>()
            .Any(e => e.Message.Contains("is not allowed to depend on solution assembly")).ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeValidWhenAssemblyDependsOnAllowedSolutionAssembly()
    {
        var mainAssembly = typeof(SolutionArchitectureBuilder).Assembly;
        var testAssembly = Assembly.GetExecutingAssembly();

        SolutionArchitectureBuilder.ForAssemblies(mainAssembly, testAssembly)
            .WithAssembly(testAssembly, builder =>
            {
                builder.WithAllowedSolutionDependencies(mainAssembly.GetName().Name!);
            })
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeInvalidWhenAssemblyDependsOnForbiddenSolutionAssembly()
    {
        var mainAssembly = typeof(SolutionArchitectureBuilder).Assembly;
        var testAssembly = Assembly.GetExecutingAssembly();

        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies(mainAssembly, testAssembly)
                .WithAssembly(testAssembly, builder =>
                {
                    builder.WithForbiddenSolutionDependencies(mainAssembly.GetName().Name!);
                })
                .ShouldBeValid();
        });

        exception.AssertExceptions.OfType<AssertAssembyDependencyException>()
            .Any(e => e.Message.Contains("is forbidden to depend on solution assembly")).ShouldBeTrue();
    }

    [Fact]
    public void ShouldThrowIfBothAllowedAndForbiddenDependenciesAreDefined()
    {
        var mainAssembly = typeof(SolutionArchitectureBuilder).Assembly;
        var testAssembly = Assembly.GetExecutingAssembly();

        Should.Throw<InvalidOperationException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies(mainAssembly, testAssembly)
                .WithAssembly(testAssembly, builder =>
                {
                    builder.WithAllowedSolutionDependencies("SomeAssembly");
                    builder.WithForbiddenSolutionDependencies("OtherAssembly");
                });
        }).Message.ShouldContain("cannot use both");
    }
}
