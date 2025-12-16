using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests.Extensions;

public static class MethodInfoExtensions
{
    extension(MethodInfo method)
    {
        public void ShouldHaveAttribute(Type attributeType)
        {
            if (!method.HasAttribute(attributeType))
                throw new AssertArchitectureException($"Method {method.Name} expected to have attribute {attributeType.FullName}");
        }

        private bool HasAttribute(Type attributeType)
        {
            var attributes = method.GetCustomAttributes(true);
            return attributes.Any(attr => attr.GetType() == attributeType);
        }
    }
}