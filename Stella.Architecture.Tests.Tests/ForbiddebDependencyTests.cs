
using System.Reflection;
using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App;

namespace Stella.Architecture.Tests.Tests;

public class ForbiddenDependencyTests
{
  [Fact]
  public void ShouldBeInvalidWhenForbiddenDependencyExists()
  {
    var exception = Should.Throw<AssertArchitectureException>(() =>
    {
      AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
        .WithForbiddenAssemblyDependency("Newtonsoft")
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
      .WithForbiddenAssemblyDependency("DummyDependency")
      .ShouldBeValid();
  }
}
