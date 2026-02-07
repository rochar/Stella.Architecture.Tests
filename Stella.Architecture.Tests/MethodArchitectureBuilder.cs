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


    internal static MethodArchitectureBuilder ForMethod(Type type, MethodInfo methodInfo)
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

    public IEnumerable<AssertMethodInvalidException> ShouldBeValid(Type type)
    {
        if (_inValidationType.IsInterface)
        {
            var implementingMethod = GetMethod(type);
            if (implementingMethod == null)
            {
                yield return new AssertMethodInvalidException(
                    $"Method {_methodInfo.Name} in type {type.FullName} not found!", type, _methodInfo);
                yield break;
            }

            foreach (var reqAtt in _requiredAttributes)
                if (!implementingMethod.HasAttribute(reqAtt))
                    yield return new AssertMethodInvalidException(
                        $"Method {implementingMethod.Name} in type {type.FullName} expected to have attribute {reqAtt.FullName}",
                        type, implementingMethod);
        }
        else
        {
            foreach (var reqAtt in _requiredAttributes)
                if (!_methodInfo.HasAttribute(reqAtt))
                    yield return new AssertMethodInvalidException(
                        $"Method {_methodInfo.Name} expected to have attribute {reqAtt.FullName}", _inValidationType,
                        _methodInfo);
        }
    }

    private MethodInfo? GetMethod(Type type)
    {
        var interfaceType = _inValidationType;
        // Handle open generic types
        if (_inValidationType.IsGenericTypeDefinition)
            // Find the closed generic interface implemented by the type
            interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == _inValidationType);

        var interfaceMap = type.GetInterfaceMap(interfaceType);

        var methodIndex = FindMethodInInterface(interfaceMap);

        if (methodIndex == -1)
            return null;

        return interfaceMap.TargetMethods[methodIndex];
    }

    private int FindMethodInInterface(InterfaceMapping interfaceMap)
    {
        for (var i = 0; i < interfaceMap.InterfaceMethods.Length; i++)
            if (MethodSignatureMatches(interfaceMap.InterfaceMethods[i], _methodInfo))
                return i;

        return -1;
    }

    private static bool MethodSignatureMatches(MethodInfo method1, MethodInfo method2)
    {
        if (method1.Name != method2.Name)
            return false;

        var params1 = method1.GetParameters();
        var params2 = method2.GetParameters();

        if (params1.Length != params2.Length)
            return false;

        for (var i = 0; i < params1.Length; i++)
            if (!params2[i].ParameterType.IsGenericParameter && params1[i].ParameterType != params2[i].ParameterType)
                return false;

        return true;
    }
}