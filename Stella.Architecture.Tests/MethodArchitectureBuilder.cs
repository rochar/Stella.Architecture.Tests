using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public class MethodArchitectureBuilder
{
    private readonly Type _type;
    private readonly MethodInfo _methodInfo;
    private readonly List<Type> _requiredAttributes = [];

    private MethodArchitectureBuilder(Type type, MethodInfo methodInfo)
    {
        _type = type;
        _methodInfo = methodInfo;
    }


    public static MethodArchitectureBuilder ForMethod(Type type, MethodInfo methodInfo)
    {
        return new MethodArchitectureBuilder(type, methodInfo);
    }

    /// <summary>
    /// Validates that the method has a specific attribute.
    /// </summary>
    public MethodArchitectureBuilder WithRequiredAttribute(Type attributeType)
    {
        _requiredAttributes.Add(attributeType);
        return this;
    }

    public IEnumerable<AssertMethodInvalidException> ShouldBeValid(Type[] allTypes)
    {
        foreach (var reqAtt in _requiredAttributes)
            if (!_methodInfo.HasAttribute(reqAtt))
                yield return new AssertMethodInvalidException(
                    $"Method {_methodInfo.Name} expected to have attribute {reqAtt.FullName}", _type, _methodInfo);
    }
}