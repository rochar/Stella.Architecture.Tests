using Shouldly;
using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
using Stella.Architecture.Tests.Tests.App;
namespace Stella.Architecture.Tests.Tests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void ShouldBeRecord()
    {
        typeof(ARecord).ShouldBeRecord();
    }
    [Fact]
    public void ShouldBeRecordFailsWhenClass()
    {
        var exception = Should.Throw<AssertArchitectureException>(() =>
        {
            typeof(AClass).ShouldBeRecord();
        });
        exception.Message.ShouldBe("Stella.Architecture.Tests.Tests.App.AClass expected to be a record");
    }
}
