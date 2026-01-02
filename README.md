# Stella.Architecture.Tests

[![CI/CD](https://github.com/rochar/Stella.Architecture.Tests/actions/workflows/ci.yml/badge.svg)](https://github.com/rochar/Stella.Architecture.Tests/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Stella.Architecture.Tests.svg)](https://www.nuget.org/packages/Stella.Architecture.Tests)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stella.Architecture.Tests.svg)](https://www.nuget.org/packages/Stella.Architecture.Tests)

A lightweight .NET library for writing architecture tests: verify layering, dependencies, naming conventions, and modular boundaries with a simple fluent API and extension methods.

## Why Architecture Tests?

Architecture tests help you:
- **Enforce architectural boundaries** between layers and modules
- **Prevent unwanted dependencies** from creeping into your codebase
- **Catch violations early** in the development process
- **Document architectural decisions** as executable tests

## Features

- ✅ **Namespace isolation** - Enforce isolated modules with no inbound/outbound dependencies
- ✅ **Assembly dependencies** - Prevent forbidden assembly references
- ✅ **Type validation** - Validate type characteristics (e.g., records)
- ✅ **Fluent API** - Readable and maintainable test syntax
- ✅ **Comprehensive error reporting** - Clear violation messages
- ✅ **No dependencies** - Lightweight library that integrates with any test framework

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Stella.Architecture.Tests
```

Or via Package Manager Console:

```powershell
Install-Package Stella.Architecture.Tests
```

## Quick Start

Ensure a namespace doesn't depend on other parts of your assembly:


```csharp
using Stella.Architecture.Tests;
using System.Reflection;
using Xunit;

public class ArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnOtherLayers_WhenInDomainLayer()
    {
        AssemblyArchitectureBuilder
            .ForAssembly(Assembly.Load("MyApp"))
            .WithNamespaceNoOutboundDependencies("MyApp.Domain")
            .ShouldBeValid();
    }
}
```
This test ensures that types in the `MyApp.Domain` namespace (and its child namespaces) do not reference any types from other namespaces in the same assembly. For example:

## Usage Examples

### Testing Namespace Isolation

Ensure a namespace is completely isolated with no dependencies to or from other namespaces  (of your assembly):

```csharp
[Fact]
public void ShouldBeIsolated_WhenInPluginNamespace()
{
    AssemblyArchitectureBuilder
        .ForAssembly(Assembly.Load("MyApp"))
        .WithNamespaceIsolated("MyApp.Plugin")
        .ShouldBeValid();
}
```

### Testing No Inbound Dependencies

Ensure all namespaces do not depend on this namespace (of your assembly):


```csharp
[Fact]
public void ShouldNotBeReferenced_WhenInInternalNamespace()
{
    AssemblyArchitectureBuilder
        .ForAssembly(Assembly.Load("MyApp"))
        .WithNamespaceNoInboundDependencies("MyApp.Internal")
        .ShouldBeValid();
}
```



### Testing Forbidden Assembly Dependencies

Prevent your application from depending on specific assemblies:

```csharp
[Fact]
public void ShouldNotDependOnLegacyLibraries_WhenInCoreAssembly()
{
    AssemblyArchitectureBuilder
        .ForAssembly(Assembly.Load("MyApp"))
        .WithAssemblyForbiddenDependency("Newtonsoft") // Regex pattern        
        .ShouldBeValid();
}
```

### Testing Type Characteristics

Validate that types follow specific patterns:

```csharp
[Fact]
public void ShouldBeRecords_WhenImplementingIDto()
{
    AssemblyArchitectureBuilder
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IDto>(builder => builder.IsRecord())
        .ShouldBeValid();
}

[Fact]
public void ShouldNotBeRecords_WhenImplementingIEntity()
{
    AssemblyArchitectureBuilder
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IEntity>(builder => builder.IsNotRecord())
        .ShouldBeValid();
}
```

