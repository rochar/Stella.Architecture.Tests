using Shouldly;
using Stella.Architecture.Tests.Tests.App.Salmon;
using Stella.Architecture.Tests.Tests.App.Tuna;
using Stella.Architecture.Tests.Tests.App.Tuna.Atlantic;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class AssemblyArchitectureBuilderTests
{
    [Fact]
    public void ShouldBeInvalidIsolatedNamespace()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithIsolatedNamespace("Stella.Architecture.Tests.Tests.App.Tuna")
                .ShouldBeValid();
        });

        var exceptions = exception.AssertArchitectureExceptions.OfType<AssertInvalidDependencyException>().ToArray();

        exceptions.Length.ShouldBe(4);

        exceptions.ShouldContain(e => e.CurrentType == typeof(Tuna) && e.ReferencedType == typeof(Salmon));
        exceptions.ShouldContain(e => e.CurrentType == typeof(TunaField) && e.ReferencedType == typeof(Salmon));
        exceptions.ShouldContain(e => e.CurrentType == typeof(TunaProperty) && e.ReferencedType == typeof(Salmon));
        exceptions.ShouldContain(e => e.CurrentType == typeof(AtlanticTuna) && e.ReferencedType == typeof(Salmon));
    }

    [Fact]
    public void ShouldBeVaLidIsolatedNamespace()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithIsolatedNamespace("Stella.Architecture.Tests.Tests.App.Sardine")
            .ShouldBeValid();
    }
}