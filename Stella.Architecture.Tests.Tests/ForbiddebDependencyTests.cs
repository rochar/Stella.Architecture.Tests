
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
        .WithForbiddenDependency("Newtonsoft")
        .ShouldBeValid();
    });

    exception.AssertInvalidDependencyExceptions.Length.ShouldBe(1);
    exception.AssertInvalidDependencyExceptions.First().CurrentType.ShouldBe(typeof(AForbiddenDependencyClass));
    exception.AssertInvalidDependencyExceptions.First().ReferencedType.ShouldBe(typeof(Newtonsoft.Json.JsonSerializer));
  }

  [Fact]
  public void ShouldBeValidWhenForbiddenDependencyExistsNotFound()
  {
    AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
      .WithForbiddenDependency("DummyDependency")
      .ShouldBeValid();
  }
}
