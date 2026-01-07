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
    public void ShouldBeValidWhenConcreteFromGenericInterfaceWithHasRequiredAttribute()
    {
        var methodInfo = typeof(IGenericTypeWithAttribute<,>).GetMethod(nameof(IGenericTypeWithAttribute<object, object>.WithoutGenericParameters));

        AssemblyArchitectureBuilder.ForAssembly(Assembly.GetExecutingAssembly())
            .WithType(typeof(IGenericTypeWithAttribute<,>), configure =>
                configure.WithMethod(methodInfo!,
                    configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
            .ShouldBeValid();
    }
    [Fact]
    public void ShouldBeValidWhenConcreteFromGenericInterfaceWithGenericParametersHasRequiredAttribute()
    {
        var methodInfo = typeof(IGenericTypeWithAttribute<,>).GetMethod(nameof(IGenericTypeWithAttribute<object, object>.WithGenericParameters));

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
    [TestForMethod]
    public Task<string> WithGenericParameters(string query, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }
    [TestForMethod]
    public string WithoutGenericParameters(string query, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
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
internal interface IGenericTypeWithAttribute<TQuery, TResult>
{
    Task<TResult> WithGenericParameters(TQuery query, CancellationToken cancellationToken = default(CancellationToken));

    TResult WithoutGenericParameters(string query, CancellationToken cancellationToken = default(CancellationToken));
}

#endregion