using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App;
using Stella.Architecture.Tests.Tests.App.Sardine;
using Stella.Architecture.Tests.Tests.App.Tuna;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class DependencyTests
{
    [Fact]
    public void ShouldBeInvalidDependency()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithDependencyUsedOnly<Tuna>(typeof(Sardine))
                .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBe(1);
        var dependencyException = exception.AssertExceptions.OfType<AssertTypeDependencyException>().Single();
        dependencyException.CurrentType.ShouldBe(typeof(DependsOnTuna));
        dependencyException.ReferencedType.ShouldBe(typeof(Tuna));
    }

    [Fact]
    public void ShouldBeValidDependency()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithDependencyUsedOnly<Tuna>(typeof(DependsOnTuna))
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeValidDependencyWhenExtensionClass()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithDependencyUsedOnly<DependencyOnExtension>(typeof(DependencyOnExtensionExtensions))
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeValidDependencyWhenClosureClass()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithDependencyUsedOnly<DependencyInClosure>(typeof(ClassWithLambdaClosure))
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeValidDependencyWhenInterface()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithDependencyUsedOnly<IDependency>(typeof(IDependant))
                .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBe(1);
        var dependencyException = exception.AssertExceptions.OfType<AssertTypeDependencyException>().Single();
        dependencyException.CurrentType.ShouldBe(typeof(InvalidDependant));
        dependencyException.ReferencedType.ShouldBe(typeof(IDependency));
    }

    #region App Classes

    public class Dependant : IDependant
    {
        private readonly IDependency _dependency;

        public Dependant(IDependency dependency)
        {
            _dependency = dependency;
        }
    }

    public class InvalidDependant
    {
        public InvalidDependant(IDependency dependency)
        {
        }
    }

    public interface IDependant
    {
    }

    public interface IDependency
    {
    }
    #endregion
}

public class DependencyOnExtension
{
}

public static class DependencyOnExtensionExtensions
{
    public static string DummyExtension(this DependencyOnExtension dependant)
    {
        return dependant.ToString() + "Dummy";
    }
}

public class DependencyInClosure
{
    public string Name { get; set; } = "Dependency";
}

public class ClassWithLambdaClosure
{
    public List<string> ProcessItems(List<string> items)
    {
        // This creates a compiler-generated closure class (<>c__DisplayClass) 
        // that captures the DependencyInClosure instance
        var dependency = new DependencyInClosure();

        return items.Where(item => item.Contains(dependency.Name)).ToList();
    }
}