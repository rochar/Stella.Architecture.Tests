using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
using System.Linq.Expressions;

namespace Stella.Architecture.Tests;

/// <summary>
/// Provides a fluent API for configuring and validating architectural rules for types in an assembly.
/// Enables validation of type characteristics, naming conventions, and namespace patterns for all types assignable from the configured type.
/// </summary>
public sealed class TypeArchitectureBuilder<T> : ITypeArchitectureBuilder
{
    private readonly Type _type;
    private bool? _isRecord;
    private System.Text.RegularExpressions.Regex? _nameRegex;
    private System.Text.RegularExpressions.Regex? _namespaceRegex;
    private readonly List<MethodArchitectureBuilder> _methodBuilders = [];


    private TypeArchitectureBuilder(Type type)
    {
        _type = type;
    }


    public static TypeArchitectureBuilder<T> ForType()
    {
        return new TypeArchitectureBuilder<T>(typeof(T));
    }

    /// <summary>
    /// Validates that the type must be a record.
    /// </summary>
    public TypeArchitectureBuilder<T> IsRecord()
    {
        _isRecord = true;
        return this;
    }

    /// <summary>
    /// Validates that the type must not be a record.
    /// </summary>
    public TypeArchitectureBuilder<T> IsNotRecord()
    {
        _isRecord = false;
        return this;
    }


    /// <summary>
    /// Validates architectural rules for a specific method on the type.
    /// </summary>
    /// <param name="methodSelector">Expression that selects the method to validate.</param>
    /// <param name="configure">Configuration action for method validation rules</param>
    public TypeArchitectureBuilder<T> WithMethod(Expression<Action<T>> methodSelector,
        Action<MethodArchitectureBuilder> configure)
    {
        if (methodSelector.Body is MethodCallExpression methodCall)
        {
            var methodBuilder = MethodArchitectureBuilder.ForMethod(_type, methodCall.Method);
            configure(methodBuilder);
            _methodBuilders.Add(methodBuilder);
            return this;
        }

        throw new InvalidOperationException($"{nameof(methodSelector)} is not a MethodCallExpression");
    }

    /// <summary>
    /// Validates that the type name must match the given regular expression pattern.
    /// </summary>
    /// <param name="regularExpression">The regular expression pattern to match the type name.</param>
    public TypeArchitectureBuilder<T> WithNameMatch(string regularExpression)
    {
        _nameRegex = new System.Text.RegularExpressions.Regex(regularExpression,
            System.Text.RegularExpressions.RegexOptions.Compiled);
        return this;
    }

    /// <summary>
    /// Validates that the type namespace must match the given regular expression pattern.
    /// </summary>
    /// <param name="regularExpression">The regular expression pattern to match the namespace.</param>
    public TypeArchitectureBuilder<T> WithNamespaceMatch(string regularExpression)
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
    /// <returns>Enumerable of Exception for each violation.</returns>
    public IEnumerable<Exception> ShouldBeValid(Type[] allTypes)
    {
        foreach (var type in allTypes.Where(t => !t.IsInterface && _type.IsAssignableFrom(t)))
        {
            var recordEx = ShouldBeRecord(type);
            if (recordEx is not null)
                yield return recordEx;

            var nameEx = ShouldNameMatch(type);
            if (nameEx is not null)
                yield return nameEx;

            var nsEx = ShouldNamespaceMatch(type);
            if (nsEx is not null)
                yield return nsEx;

            foreach (var methodArchitectureBuilder in _methodBuilders)
            {
                foreach (var methodException in methodArchitectureBuilder.ShouldBeValid(allTypes))
                {
                    yield return methodException;
                }
            }
        }
    }

    private AssertTypeInvalidException? ShouldNameMatch(Type type)
    {
        if (_nameRegex is not null && !_nameRegex.IsMatch(type.Name))
            return new AssertTypeInvalidException(
                $"{type.FullName} Name does not match pattern '{_nameRegex}'", type);

        return null;
    }

    private AssertTypeInvalidException? ShouldBeRecord(Type type)
    {
        if (_isRecord is null)
            return null;

        if ((_isRecord!.Value && !type.IsRecord()) || (!_isRecord.Value && type.IsRecord()))
            return new AssertTypeInvalidException($"{type.FullName} is Record {type.IsRecord()}", type);

        return null;
    }

    private AssertTypeInvalidException? ShouldNamespaceMatch(Type type)
    {
        if (_namespaceRegex is not null)
        {
            var ns = type.Namespace ?? string.Empty;
            if (!_namespaceRegex.IsMatch(ns))
                return new AssertTypeInvalidException(
                    $"{type.FullName} Namespace '{ns}' does not match pattern '{_namespaceRegex}'", type);
        }

        return null;
    }
}