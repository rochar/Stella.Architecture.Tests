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
    public void ShouldBeInvalidWhenNotRecord()
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

    [Fact]
    public void ShouldBeValidWhenNameEndsWithForClass()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType<TypeWithTestName>(typeBuilder => typeBuilder.WithNameMatch("TestName"))
            .ShouldBeValid();
    }
    [Fact]
    public void ShouldBeValidWhenNameEndsWithForInterface()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType<ITypeWithTestName>(typeBuilder => typeBuilder.WithNameMatch(".*TestName$"))
            .ShouldBeValid();
    }
    [Fact]
    public void ShouldBeInvalidWhenNameDoesNotEndsWith()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<TypeBadName>(typeBuilder => typeBuilder.IsRecord())
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(2);

        exception.AssertExceptions.OfType<AssertTypeInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(DeriveTypeBadName));
    }

    [Fact]
    public void ShouldBeValidWhenNamespaceMatchs()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType<ITypeWithTestName>(typeBuilder => typeBuilder.WithNamespaceMatch("Stella.Architecture.Tests.Tests"))
            .ShouldBeValid();
    }

    [Fact]
    public void ShouldBeInvalidWhenNamespaceDoesNotMatch()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<ITypeWithTestName>(typeBuilder =>
                    typeBuilder.WithNamespaceMatch("Dummy.Namespace"))
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertTypeInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(TypeWithTestName));
    }
    [Fact]
    public void ShouldBeInvalidWhenMultipleValidations()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<ITypeWithTestName>(typeBuilder =>
                    typeBuilder.WithNamespaceMatch("Dummy.Namespace"))
                .WithType<AClass>(typeBuilder => typeBuilder.IsRecord())
                .WithType<TypeBadName>(typeBuilder => typeBuilder.IsRecord())
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(4);
    }
    internal sealed class TypeWithTestName : ITypeWithTestName
    {

    }
    internal interface ITypeWithTestName
    {
    }

    internal sealed class DeriveTypeBadName : TypeBadName
    {
    }

    internal class TypeBadName
    {
    }
}

