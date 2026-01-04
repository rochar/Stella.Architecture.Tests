using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;

namespace Stella.Architecture.Tests
{
    /// <summary>
    /// Provides a fluent API for configuring and validating architectural rules for types in an assembly.
    /// Enables validation of type characteristics, naming conventions, and namespace patterns for all types assignable from the configured type.
    /// </summary>
    public sealed class TypeArchitectureBuilder
    {
        private readonly Type _type;
        private bool? _isRecord;
        private string? _nameEndsWith;
        private System.Text.RegularExpressions.Regex? _namespaceRegex;

        private TypeArchitectureBuilder(Type type)
        {
            _type = type;
        }


        public static TypeArchitectureBuilder ForType(Type type)
        {
            return new TypeArchitectureBuilder(type);
        }

        /// <summary>
        /// Validates that the type must be a record.
        /// </summary>
        public TypeArchitectureBuilder IsRecord()
        {
            _isRecord = true;
            return this;
        }

        /// <summary>
        /// Validates that the type must not be a record.
        /// </summary>
        public TypeArchitectureBuilder IsNotRecord()
        {
            _isRecord = false;
            return this;
        }

        /// <summary>
        /// Validates that the type name must end with the given suffix.
        /// </summary>
        /// <param name="nameEndsWith">The required suffix for the type name.</param>
        public TypeArchitectureBuilder WithNameEndsWith(string nameEndsWith)
        {
            _nameEndsWith = nameEndsWith;
            return this;
        }

        /// <summary>
        /// Validates that the type namespace must match the given regular expression pattern.
        /// </summary>
        /// <param name="regularExpression">The regular expression pattern to match the namespace.</param>
        public TypeArchitectureBuilder WithNamespaceMatch(string regularExpression)
        {
            _namespaceRegex = new System.Text.RegularExpressions.Regex(regularExpression,
                System.Text.RegularExpressions.RegexOptions.Compiled);
            return this;
        }

        /// <summary>
        /// Validates all types assignable from the configured type against the specified rules.
        /// Returns all invalid type exceptions found.
        /// </summary>
        /// <param name="allTypes">All types to validate.</param>
        /// <returns>Enumerable of AssertTypeInvalidException for each violation.</returns>
        public IEnumerable<AssertTypeInvalidException> ShouldBeValid(Type[] allTypes)
        {
            foreach (var type in allTypes.Where(t => !t.IsInterface && _type.IsAssignableFrom(t)))
            {
                var recordEx = ShouldBeRecord(type);
                if (recordEx is not null)
                    yield return recordEx;

                var nameEx = ShouldNameEndsWith(type);
                if (nameEx is not null)
                    yield return nameEx;

                var nsEx = ShouldNamespaceMatch(type);
                if (nsEx is not null)
                    yield return nsEx;
            }
        }

        private AssertTypeInvalidException? ShouldNameEndsWith(Type type)
        {
            if (_nameEndsWith is not null && !type.Name.EndsWith(_nameEndsWith))
                return new AssertTypeInvalidException(
                    $"{type.FullName} Name does not ends with {_nameEndsWith}", type);
            return null;
        }

        private AssertTypeInvalidException? ShouldBeRecord(Type type)
        {
            if (_isRecord is null)
                return null;

            if ((_isRecord!.Value && !type.IsRecord()) || (!_isRecord.Value && type.IsRecord()))
            {
                return new AssertTypeInvalidException($"{type.FullName} is Record {type.IsRecord()}", type);
            }

            return null;
        }

        private AssertTypeInvalidException? ShouldNamespaceMatch(Type type)
        {
            if (_namespaceRegex is not null)
            {
                var ns = type.Namespace ?? string.Empty;
                if (!_namespaceRegex.IsMatch(ns))
                {
                    return new AssertTypeInvalidException(
                        $"{type.FullName} Namespace '{ns}' does not match pattern '{_namespaceRegex}'", type);
                }
            }

            return null;
        }
    }
}