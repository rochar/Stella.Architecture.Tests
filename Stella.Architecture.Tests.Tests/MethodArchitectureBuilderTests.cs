using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class MethodArchitectureBuilderTests
{
    [Fact]
    public void ShouldBeInvalidValidWhenMethodWithoutRequiredAttribute()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<TypeWithAttribute>(configure =>
                    configure.WithMethod<TypeWithAttribute>((t) => t.MethodWithoutAttribute(),
                        configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertMethodInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(TypeWithAttribute) && e.Message ==
            "Method MethodWithoutAttribute expected to have attribute Stella.Architecture.Tests.Tests.TestForMethodAttribute");
    }

    [Fact]
    public void ShouldBeValidWhenHasRequiredAttribute()
    {
        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType<TypeWithAttribute>(configure =>
                configure.WithMethod<TypeWithAttribute>(t => t.MethodWithAttribute(),
                    configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
            .ShouldBeValid();
    }


    [Fact]
    public void ShouldBeInvalidValidWhenConcreteTypeMethodWithoutRequiredAttribute()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
                .WithType<ITypeWithAttribute>(configure =>
                    configure.WithMethod<ITypeWithAttribute>((t) => t.MethodWithAttribute(),
                        configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
                .ShouldBeValid();
        });


        exception.AssertExceptions.Length.ShouldBe(1);

        exception.AssertExceptions.OfType<AssertMethodInvalidException>().ShouldContain(e =>
            e.CurrentType == typeof(TypeWithoutAttribute));
    }
    [Fact]
    public void ShouldBeValidWhenConcreteFromGenericInterfaceHasRequiredAttribute()
    {
        var methodInfo = typeof(IGenericTypeWithAttribute<,>).GetMethod(nameof(IGenericTypeWithAttribute<object, object>.MethodWithAttribute));

        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType(typeof(IGenericTypeWithAttribute<,>), configure =>
                configure.WithMethod(methodInfo!,
                    configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
            .ShouldBeValid();
    }
}

#region App Test Classes

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class TestForMethodAttribute : Attribute
{
}

internal sealed class TypeWithAttribute : ITypeWithAttribute, IGenericTypeWithAttribute<string, string>
{
    private readonly int _dummy = 1;

    [TestForMethod]
    public int MethodWithAttribute()
    {
        return _dummy;
    }

    public int MethodWithoutAttribute()
    {
        return _dummy;
    }
}
internal sealed class TypeWithoutAttribute : ITypeWithAttribute
{
    private readonly int _dummy = 1;

    public int MethodWithAttribute()
    {
        return _dummy;
    }
}

internal interface ITypeWithAttribute
{
    int MethodWithAttribute();
}
internal interface IGenericTypeWithAttribute<T1, T2> : ITypeWithAttribute
{
    int MethodWithAttribute();
}

#endregion