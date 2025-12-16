using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App;
using Stella.Architecture.Tests.Tests.App.Tuna;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class NoInboundDependenciesInNamespaceTests
{
    [Fact]
    public void ShouldBeInvalidInboundDependency()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithNoInboundDependenciesInNamespace("Stella.Architecture.Tests.Tests.App.Tuna")
                .ShouldBeValid();
        });

        var exceptions = exception.AssertArchitectureExceptions.OfType<AssertInvalidDependencyException>().ToArray();

        exceptions.Length.ShouldBe(1);

        exceptions.ShouldContain(e => e.CurrentType == typeof(DependsOnTuna) && e.ReferencedType == typeof(Tuna));
    }

    [Fact]
    public void ShouldNotHaveInboundDependency()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithNoInboundDependenciesInNamespace("Stella.Architecture.Tests.Tests.App.Sardine")
            .ShouldBeValid();
    }
}