namespace Stella.Architecture.Tests.Tests.App;

public class AClass
{
    [Obsolete("Obsolete")]
    public void MethodWithObsoleteAttribute()
    {
    }

#pragma warning disable CA1822
    public void MethodWithoutAttributes()
#pragma warning restore CA1822
    {
    }
}