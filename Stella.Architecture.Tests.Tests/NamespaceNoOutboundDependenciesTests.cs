using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App.Salmon;
using Stella.Architecture.Tests.Tests.App.Tuna;
using Stella.Architecture.Tests.Tests.App.Tuna.Atlantic;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class NamespaceNoOutboundDependenciesTests
{
    [Fact]
    public void ShouldBeInvalidIsolatedNamespace()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithNamespaceNoOutboundDependencies("Stella.Architecture.Tests.Tests.App.Tuna")
                .ShouldBeValid();
        });


        var dependencyExceptions = exception.AssertExceptions.OfType<AssertTypeDependencyException>().ToArray();
        dependencyExceptions.ShouldContain(e =>
            e.CurrentType == typeof(Tuna) && e.ReferencedType == typeof(Salmon));
        dependencyExceptions.ShouldContain(e =>
            e.CurrentType == typeof(TunaField) && e.ReferencedType == typeof(Salmon));
        dependencyExceptions.ShouldContain(e =>
            e.CurrentType == typeof(TunaProperty) && e.ReferencedType == typeof(Salmon));
        dependencyExceptions.ShouldContain(e =>
            e.CurrentType == typeof(AtlanticTuna) && e.ReferencedType == typeof(Salmon));
    }

    [Fact]
    public void ShouldBeVaLidIsolatedNamespace()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithNamespaceNoOutboundDependencies("Stella.Architecture.Tests.Tests.App.Sardine")
            .ShouldBeValid();
    }
}