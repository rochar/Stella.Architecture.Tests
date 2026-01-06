namespace Stella.Architecture.Tests;

internal interface ITypeArchitectureBuilder
{
    IEnumerable<Exception> ShouldBeValid(Type[] allTypes);
}