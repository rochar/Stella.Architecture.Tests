using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;

namespace Stella.Architecture.Tests
{
    public sealed class TypeArchitectureBuilder
    {
        private readonly Type _type;
        private bool? _isRecord;
        private string? _nameEndsWith;

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

        public TypeArchitectureBuilder WithNameEndsWith(string nameEndsWith)
        {
            _nameEndsWith = nameEndsWith;
            return this;
        }

        public IEnumerable<AssertTypeInvalidException> ShouldBeValid(Type[] allTypes)
        {
            foreach (Type type in allTypes)
            {
                if (type.IsInterface || !_type.IsAssignableFrom(type))
                    continue;
                
                if (_isRecord is not null)
                {
                    var recordException = ValidateRecord(type);
                    if (recordException != null)
                        yield return recordException;
                }

                if (_nameEndsWith is not null && !type.Name.EndsWith(_nameEndsWith))
                    yield return new AssertTypeInvalidException(
                        $"{type.FullName} Name does not ends with {_nameEndsWith}", type);
            }
        }

        private AssertTypeInvalidException? ValidateRecord(Type type)
        {
            if ((_isRecord!.Value && !type.IsRecord()) || (!_isRecord.Value && type.IsRecord()))
            {
                return new AssertTypeInvalidException($"{type.FullName} is Record {type.IsRecord()}", type);
            }

            return null;
        }
    }
}