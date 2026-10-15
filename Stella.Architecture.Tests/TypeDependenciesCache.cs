using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace Stella.Architecture.Tests;

internal static class TypeDependenciesCache
{
    private sealed record References(ImmutableHashSet<Type> Internal, ImmutableHashSet<Type> External);
    // Shared by every builder in the process, so concurrent tests read and fill it from many threads. A type may be
    // built twice under contention, which is harmless: the build only reads reflection metadata.
    private static readonly ConcurrentDictionary<Type, References> _typeDependencies = new();

    internal static ImmutableHashSet<Type> GetInternalReferenceTypes(Type type) =>
        _typeDependencies.GetOrAdd(type, BuildReferences).Internal;

    internal static ImmutableHashSet<Type> GetExternalReferenceTypes(Type type) =>
        _typeDependencies.GetOrAdd(type, BuildReferences).External;

    private static References BuildReferences(Type type)
    {
        var assembly = type.Assembly;

        var referencedTypes = ImmutableHashSet.CreateBuilder<Type>();
        var externalReferencedTypes = ImmutableHashSet.CreateBuilder<Type>();

        if (type is { BaseType: not null } && type.BaseType != typeof(object))
        {
            if (type.BaseType.Assembly == assembly)
                referencedTypes.Add(type.BaseType);
            else
                externalReferencedTypes.Add(type.BaseType);
        }

        foreach (var @interface in type.GetInterfaces())
        {
            if (@interface.Assembly == assembly)
                referencedTypes.Add(@interface);
            else
                externalReferencedTypes.Add(@interface);
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                             BindingFlags.Static))
        {
            if (field.FieldType.Assembly == assembly)
                referencedTypes.Add(field.FieldType);
            else
                externalReferencedTypes.Add(field.FieldType);
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                    BindingFlags.Instance | BindingFlags.Static))
        {
            if (property.PropertyType.Assembly == assembly)
                referencedTypes.Add(property.PropertyType);
            else
                externalReferencedTypes.Add(property.PropertyType);
        }

        foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic |
                                                         BindingFlags.Instance | BindingFlags.Static))
        {
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.ParameterType.Assembly == assembly)
                    referencedTypes.Add(parameter.ParameterType);
                else
                    externalReferencedTypes.Add(parameter.ParameterType);
            }
        }

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (method.ReturnType != null)
            {
                if (method.ReturnType.Assembly == assembly)
                    referencedTypes.Add(method.ReturnType);
                else
                    externalReferencedTypes.Add(method.ReturnType);
            }
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.ParameterType.Assembly == assembly)
                    referencedTypes.Add(parameter.ParameterType);
                else
                    externalReferencedTypes.Add(parameter.ParameterType);
            }
        }
        return new References(referencedTypes.ToImmutable(), externalReferencedTypes.ToImmutable());
    }
}