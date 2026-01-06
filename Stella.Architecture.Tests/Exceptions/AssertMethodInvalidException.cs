using System.Reflection;

namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertMethodInvalidException(string message, Type? currentType, MethodInfo methodInfo)
    : Exception(message)
{
    public Type? CurrentType { get; } = currentType;
    public MethodInfo MethodInfo { get; } = methodInfo;
}