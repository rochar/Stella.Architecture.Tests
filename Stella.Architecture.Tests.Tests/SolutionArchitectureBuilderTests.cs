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
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            SolutionArchitectureBuilder.ForAssemblies()
                .WithAssembly(Assembly.GetExecutingAssembly(), builder =>
                {
                    builder.WithAssemblyForbiddenDependency("Newtonsoft");
                })
                .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBeGreaterThanOrEqualTo(1);
    }
}
