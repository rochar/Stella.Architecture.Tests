using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
using System.Reflection;

namespace Stella.Architecture.Tests;

public class MethodArchitectureBuilder
{
    private readonly Type _inValidationType;
    private readonly MethodInfo _methodInfo;
    private readonly List<Type> _requiredAttributes = [];

    private MethodArchitectureBuilder(Type inValidationType, MethodInfo methodInfo)
    {
        _inValidationType = inValidationType;
        _methodInfo = methodInfo;
    }


    public static MethodArchitectureBuilder ForMethod(Type type, MethodInfo methodInfo)
    {
        return new MethodArchitectureBuilder(type, methodInfo);
    }

    /// <summary>
    /// Validates that the method has a specific attribute in the type.
    /// If the type is interface, it checks the implementing methods in all derived types.
    /// </summary>
    public MethodArchitectureBuilder WithRequiredAttribute(Type attributeType)
    {
        _requiredAttributes.Add(attributeType);
        return this;
    }

    public IEnumerable<AssertMethodInvalidException> ShouldBeValid(Type[] allTypes)
    {
        if (_inValidationType.IsInterface)
        {
            foreach (var type in allTypes)
            {
                if (!_inValidationType.IsAssignableFrom(type) || type.IsInterface)
                    continue;

                var interfaceMap = type.GetInterfaceMap(_inValidationType);
                var methodIndex = Array.IndexOf(interfaceMap.InterfaceMethods, _methodInfo);

                if (methodIndex == -1)
                    continue;

                var implementingMethod = interfaceMap.TargetMethods[methodIndex];

                foreach (var reqAtt in _requiredAttributes)
                    if (!implementingMethod.HasAttribute(reqAtt))
                        yield return new AssertMethodInvalidException(
                            $"Method {implementingMethod.Name} in type {type.FullName} expected to have attribute {reqAtt.FullName}",
                            type, implementingMethod);
            }
        }
        else
        {
            foreach (var reqAtt in _requiredAttributes)
                if (!_methodInfo.HasAttribute(reqAtt))
                    yield return new AssertMethodInvalidException(
                        $"Method {_methodInfo.Name} expected to have attribute {reqAtt.FullName}", _inValidationType, _methodInfo);
        }
    }
}