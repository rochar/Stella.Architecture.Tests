using Shouldly;
using Stella.Architecture.Tests.Tests.App;

namespace Stella.Architecture.Tests.Tests;

public class MethodInfoExtensionsTests
{

    [Fact]
    public void ShouldHaveAttributePassesWhenMethodHasAttribute()
    {
        var method = typeof(AClass).GetMethod(nameof(AClass.MethodWithObsoleteAttribute));
        method.ShouldHaveAttribute(typeof(ObsoleteAttribute));
    }

    [Fact]
    public void ShouldHaveAttributeFailsWhenMethodDoesNotHaveAttribute()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            var method = typeof(AClass).GetMethod(nameof(AClass.MethodWithoutAttributes));
            method.ShouldHaveAttribute(typeof(ObsoleteAttribute));
        });
        exception.Message.ShouldBe("Method MethodWithoutAttributes expected to have attribute System.ObsoleteAttribute");
    }
}
