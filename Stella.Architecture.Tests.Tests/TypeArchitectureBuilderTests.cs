using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
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


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertTypeInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(TypeBadName));
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


        exception.AssertExceptions.Length.ShouldBe(3);
    }
    [Theory]
    [InlineData(typeof(TypeWithTestName), AccessModifierType.Internal)]
    [InlineData(typeof(PublicType), AccessModifierType.Public)]
    [InlineData(typeof(ProtectedType), AccessModifierType.Protected)]
    public void ShouldHaveModifier(Type type, AccessModifierType accessModifierType)
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType(type, typeBuilder =>
                typeBuilder.WithAccessModifier(accessModifierType))
            .ShouldBeValid();
    }
    [Fact]
    public void ShouldBeInvalidModifier()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<PublicType>(typeBuilder =>
                    typeBuilder.WithAccessModifier(AccessModifierType.Internal))
                .ShouldBeValid();
        });

        exception.AssertExceptions.Length.ShouldBe(1);
        exception.AssertExceptions.First().GetType().ShouldBe(typeof(AssertTypeInvalidException));
        exception.AssertExceptions.First().Message.ShouldBe("Stella.Architecture.Tests.Tests.TypeArchitectureBuilderTests+PublicType has modifier 'Public' but expected 'Internal'");
    }
    public sealed class PublicType : ITypeWithTestName
    {

    }
    protected sealed class ProtectedType : ITypeWithTestName
    {

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

