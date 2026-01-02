using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;

namespace Stella.Architecture.Tests
{
    public sealed class TypeArchitectureBuilder
    {
        private readonly Type _type;
        private bool? _isRecord = null;

        private TypeArchitectureBuilder(Type type)
        {
            _type = type;
        }

        public static TypeArchitectureBuilder ForType(Type type)
        {
            return new TypeArchitectureBuilder(type);
        }

        public TypeArchitectureBuilder IsRecord()
        {
            _isRecord = true;
            return this;
        }
        public TypeArchitectureBuilder IsNotRecord()
        {
            _isRecord = false;
            return this;
        }

        public IEnumerable<AssertTypeInvalidException> ShouldBeValid(Type[] allTypes)
        {
            if (!_isRecord.HasValue) yield break;

            foreach (Type type in allTypes)
            {
                if (type.IsInterface || !_type.IsAssignableFrom(type)) continue;

                if ((_isRecord.Value && !type.IsRecord()) || (!_isRecord.Value && type.IsRecord()))
                    yield return new AssertTypeInvalidException($"{type.FullName} is Record {type.IsRecord()}", type);
            }
        }
    }
}