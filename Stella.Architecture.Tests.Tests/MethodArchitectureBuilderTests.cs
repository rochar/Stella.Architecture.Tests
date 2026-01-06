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
                    configure.WithMethod(t => t.MethodWithoutAttribute(),
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
                configure.WithMethod(t => t.MethodWithAttribute(),
                    configureMethod => configureMethod.WithRequiredAttribute(typeof(TestForMethodAttribute))))
            .ShouldBeValid();
    }
}

#region App Test Classes

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class TestForMethodAttribute : Attribute
{
}

internal sealed class TypeWithAttribute
{
    private readonly int _dummy = 1;

    [TestForMethodAttribute]
    public int MethodWithAttribute()
    {
        return _dummy;
    }

    public int MethodWithoutAttribute()
    {
        return _dummy;
    }
}

#endregion