using Stella.Architecture.Tests.Exceptions;

namespace Stella.Architecture.Tests.Extensions;

public static class TypeExtensions
{
    extension(Type type)
    {
        public bool IsRecord()
        {
            return type.GetProperty("EqualityContract",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null;
        }

        public void ShouldBeRecord()
        {
            if (!type.IsRecord())
                throw new AssertTypeInvalidException($"{type.FullName} expected to be a record", type);
        }
        public AccessModifierType GetModifierType()
        {
            if (type.IsNested)
            {
                if (type.IsNestedPublic)
                    return AccessModifierType.Public;
                if (type.IsNestedPrivate)
                    return AccessModifierType.Private;
                if (type.IsNestedFamily)
                    return AccessModifierType.Protected;
                if (type.IsNestedAssembly)
                    return AccessModifierType.Internal;
            }
            else
            {
                if (type.IsPublic)
                    return AccessModifierType.Public;
                if (type.IsNotPublic)
                    return AccessModifierType.Internal;
            }

            return AccessModifierType.Internal;
        }
    }
}