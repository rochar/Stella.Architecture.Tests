using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Tests.App;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class TypeArchitectureBuilderTests
{
    [Fact]
    public void ShouldBeInvalidValidWhenClass()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<AClass>(typeBuilder => typeBuilder.IsRecord())
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertTypeInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(AClass));
    }

    [Fact]
    public void ShouldBeValidWhenRecord()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType<ARecord>(typeBuilder => typeBuilder.IsRecord())
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeInvalidValidWhenNotRecord()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<IShouldBeRecord>(typeBuilder => typeBuilder.IsRecord())
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertTypeInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(AClassShouldBeRecord));
    }
}