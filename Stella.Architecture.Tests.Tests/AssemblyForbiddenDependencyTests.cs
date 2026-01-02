
using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class AssemblyForbiddenDependencyTests
{
    [Fact]
    public void ShouldBeInvalidWhenForbiddenDependencyExists()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
          .WithAssemblyForbiddenDependency("Newtonsoft")
          .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBe(1);
        var dependencyException = exception.AssertExceptions.OfType<AssertAssembyDependencyException>().Single();
        dependencyException.ReferencedAssembly.FullName.ShouldStartWith("Newtonsoft");
    }

    [Fact]
    public void ShouldBeValidWhenForbiddenDependencyExistsNotFound()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
          .WithAssemblyForbiddenDependency("DummyDependency")
          .ShouldBeValid();
    }
}
