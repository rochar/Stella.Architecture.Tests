using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App;
using Stella.Architecture.Tests.Tests.App.Tuna;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class NamespaceNoInboundDependenciesTests
{
    [Fact]
    public void ShouldBeInvalidInboundDependency()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithNamespaceNoInboundDependencies("Stella.Architecture.Tests.Tests.App.Tuna")
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertTypeDependencyException>().ShouldContain(e =>
            e.CurrentType == typeof(DependsOnTuna) && e.ReferencedType == typeof(Tuna));
    }

    [Fact]
    public void ShouldNotHaveInboundDependency()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithNamespaceNoInboundDependencies("Stella.Architecture.Tests.Tests.App.Sardine")
            .ShouldBeValid();
    }
}