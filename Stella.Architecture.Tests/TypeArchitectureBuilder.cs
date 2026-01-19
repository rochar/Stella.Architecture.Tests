using Stella.Architecture.Tests.Exceptions;
using Stella.Architecture.Tests.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Stella.Architecture.Tests;

/// <summary>
/// Provides a fluent API for configuring and validating architectural rules for types in an assembly.
/// Enables validation of type characteristics, naming conventions, and namespace patterns for all types assignable from the configured type.
/// </summary>
public sealed class TypeArchitectureBuilder : ITypeArchitectureBuilder
{
    private readonly Type _inValidationType;
    private bool? _isRecord;
    private AccessModifierType? _modifierType;
    private System.Text.RegularExpressions.Regex? _nameRegex;
    private System.Text.RegularExpressions.Regex? _namespaceRegex;
    private readonly List<MethodArchitectureBuilder> _methodBuilders = [];


    private TypeArchitectureBuilder(Type inValidationType)
    {
        _inValidationType = inValidationType;
    }


    public static TypeArchitectureBuilder ForType<T>()
    {
        return new TypeArchitectureBuilder(typeof(T));
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
    /// Validates that the type must have the specified access modifier.
    /// </summary>
    /// <param name="type">The required access modifier.</param>
    public TypeArchitectureBuilder WithAccessModifier(AccessModifierType type)
    {
        _modifierType = type;
        return this;
    }

    /// <summary>
    /// Validates that the type must be public.
    /// </summary>
    public TypeArchitectureBuilder IsPublic() => WithAccessModifier(AccessModifierType.Public);

    /// <summary>
    /// Validates that the type must be internal.
    /// </summary>
    public TypeArchitectureBuilder IsInternal() => WithAccessModifier(AccessModifierType.Internal);

    /// <summary>
    /// Validates that the type must not be a record.
    /// </summary>
    public TypeArchitectureBuilder IsNotRecord()
    {
        _isRecord = false;
        return this;
    }

    /// <summary>
    /// Validates architectural rules for a specific method on the type.
    /// </summary>
    /// <param name="methodSelector">Expression that selects the method to validate.</param>
    /// <param name="configure">Configuration action for method validation rules</param>
    public TypeArchitectureBuilder WithMethod<T>(Expression<Action<T>> methodSelector,
        Action<MethodArchitectureBuilder> configure)
    {
        if (_inValidationType == typeof(T))
        {
            if (methodSelector.Body is MethodCallExpression methodCall)
            {
                var methodBuilder = MethodArchitectureBuilder.ForMethod(_inValidationType, methodCall.Method);
                configure(methodBuilder);
                _methodBuilders.Add(methodBuilder);
                return this;
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Type {typeof(T).FullName} is not assignable to configured type {_inValidationType.FullName}");
        }

        throw new InvalidOperationException($"{nameof(methodSelector)} is not a MethodCallExpression");
    }

    /// <summary>
    /// Validates architectural rules for a specific method on the type.
    /// </summary>
    /// <param name="methodSelector">Expression that selects the method to validate.</param>
    /// <param name="configure">Configuration action for method validation rules</param>
    public TypeArchitectureBuilder WithMethod(Expression<Action<object>> methodSelector,
        Action<MethodArchitectureBuilder> configure)
    {
        if (methodSelector.Body is MethodCallExpression methodCall)
        {
            var methodBuilder = MethodArchitectureBuilder.ForMethod(_inValidationType, methodCall.Method);
            configure(methodBuilder);
            _methodBuilders.Add(methodBuilder);
            return this;
        }

        throw new InvalidOperationException($"{nameof(methodSelector)} is not a MethodCallExpression");
    }

    /// <summary>
    /// Validates architectural rules for a specific method on the type.
    /// </summary>
    /// <param name="methodInfo">The MethodInfo of the method to validate. Useful for generic types where a concrete implementation cannot be provided in an expression.</param>
    /// <param name="configure">Configuration action for method validation rules</param>
    public TypeArchitectureBuilder WithMethod(MethodInfo methodInfo, Action<MethodArchitectureBuilder> configure)
    {
        var methodBuilder = MethodArchitectureBuilder.ForMethod(_inValidationType, methodInfo);
        configure(methodBuilder);
        _methodBuilders.Add(methodBuilder);
        return this;
    }

    /// <summary>
    /// Validates that the type name must match the given regular expression pattern.
    /// </summary>
    /// <param name="regularExpression">The regular expression pattern to match the type name.</param>
    public TypeArchitectureBuilder WithNameMatch(string regularExpression)
    {
        _nameRegex = new System.Text.RegularExpressions.Regex(regularExpression,
            System.Text.RegularExpressions.RegexOptions.Compiled);
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
    /// <returns>Enumerable of Exception for each violation.</returns>
    public IEnumerable<Exception> ShouldBeValid(Type[] allTypes)
    {
        if (!_inValidationType.IsInterface)
        {
            var recordEx = ShouldBeRecord(_inValidationType);
            if (recordEx is not null)
                yield return recordEx;

            var modifierEx = ShouldHaveModifier(_inValidationType);
            if (modifierEx is not null)
                yield return modifierEx;

            var nameEx = ShouldNameMatch(_inValidationType);
            if (nameEx is not null)
                yield return nameEx;

            var nsEx = ShouldNamespaceMatch(_inValidationType);
            if (nsEx is not null)
                yield return nsEx;

            foreach (var methodArchitectureBuilder in _methodBuilders)
                foreach (var methodException in methodArchitectureBuilder.ShouldBeValid(_inValidationType))
                    yield return methodException;
        }
        else //Concrete types implementing the interface
        {
            foreach (var type in allTypes.Where(ImplementsInValidationTypeInterface))
            {
                var recordEx = ShouldBeRecord(type);
                if (recordEx is not null)
                    yield return recordEx;

                var modifierEx = ShouldHaveModifier(type);
                if (modifierEx is not null)
                    yield return modifierEx;

                var nameEx = ShouldNameMatch(type);
                if (nameEx is not null)
                    yield return nameEx;

                var nsEx = ShouldNamespaceMatch(type);
                if (nsEx is not null)
                    yield return nsEx;

                foreach (var methodArchitectureBuilder in _methodBuilders)
                    foreach (var methodException in methodArchitectureBuilder.ShouldBeValid(type))
                        yield return methodException;
            }
        }
    }

    private bool ImplementsInValidationTypeInterface(Type currentType)
    {
        if (!_inValidationType.IsInterface || currentType.IsInterface)
            return false;
        if (_inValidationType.IsAssignableFrom(currentType))
            return true;

        // Check if any implemented interface is a generic type constructed from the open generic definition
        return currentType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == _inValidationType);
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

    private AssertTypeInvalidException? ShouldHaveModifier(Type type)
    {
        if (_modifierType is null)
            return null;

        var actualModifier = type.GetModifierType();
        if (actualModifier != _modifierType.Value)
            return new AssertTypeInvalidException(
                $"{type.FullName} has modifier '{actualModifier}' but expected '{_modifierType.Value}'", type);

        return null;
    }
}