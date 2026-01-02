namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertTypeInvalidException(string message, Type currentType)
    : Exception(message)
{
    public Type CurrentType { get; } = currentType;
}