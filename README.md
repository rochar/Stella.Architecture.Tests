# Stella.Architecture.Tests

[![CI/CD](https://github.com/rochar/Stella.Architecture.Tests/actions/workflows/ci.yml/badge.svg)](https://github.com/rochar/Stella.Architecture.Tests/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Stella.Architecture.Tests.svg)](https://www.nuget.org/packages/Stella.Architecture.Tests)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stella.Architecture.Tests.svg)](https://www.nuget.org/packages/Stella.Architecture.Tests)

A lightweight .NET library for writing architecture tests: verify layering, dependencies, naming conventions, and modular boundaries with a simple fluent API and extension methods.

> **Note:** The Quick Start and usage examples below use xUnit for demonstration, but you can use Stella.Architecture.Tests with any .NET test framework (such as NUnit or MSTest).

## Why Architecture Tests?

Architecture tests help you:
- **Enforce architectural boundaries** between layers and modules
- **Prevent unwanted dependencies** from creeping into your codebase
- **Catch violations early** in the development proc√ess
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
        AssemblyArchitectureconfigure
            .ForAssembly(Assembly.Load("MyApp"))
            .WithNamespaceNoOutboundDependencies("MyApp.Domain")
            .ShouldBeValid();
    }
}
```
This test ensures that types in the `MyApp.Domain` namespace (and its child namespaces) do not reference any types from other namespaces in the same assembly.

## Usage Examples

### Assembly Testing 
#### Namespace Isolation

Ensure a namespace is completely isolated with no dependencies to or from other namespaces  (of your assembly):

```csharp
[Fact]
public void ShouldBeIsolated_WhenInPluginNamespace()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithNamespaceIsolated("MyApp.Plugin")
        .ShouldBeValid();
}
```

#### Namespace No Inbound Dependencies

Ensure all namespaces do not depend on this namespace (of your assembly):


```csharp
[Fact]
public void ShouldNotBeReferenced_WhenInInternalNamespace()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithNamespaceNoInboundDependencies("MyApp.Internal")
        .ShouldBeValid();
}
```



#### Forbidden Assembly Dependencies

Prevent your application from depending on specific assemblies:

```csharp
[Fact]
public void ShouldNotDependOnLegacyLibraries_WhenInCoreAssembly()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithAssemblyForbiddenDependency("Newtonsoft") // Regex pattern        
        .ShouldBeValid();
}
```

### Type Testing

#### Is Record

```csharp
[Fact]
public void ShouldBeRecords_WhenImplementingIDto()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IDto>(configure => configure.IsRecord())
        .ShouldBeValid();
}

[Fact]
public void ShouldNotBeRecords_WhenImplementingIEntity()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IEntity>(configure => configure.IsNotRecord())
        .ShouldBeValid();
}
```

#### Restricted Type Dependencies

Ensure that only specific types can depend on a target type:

```csharp
[Fact]
public void ShouldOnlyBeUsedByServices_WhenDatabaseContext()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithDependencyUsedOnly<DatabaseContext>(typeof(IService))
        .ShouldBeValid();
}

[Fact]
public void ShouldOnlyBeUsedByApprovedServices_WhenInternalApi()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithDependencyUsedOnly<InternalApi>(typeof(Service1), typeof(Service2))
        .ShouldBeValid();
}
```

####  Type Name Match

```csharp
[Fact]
public void ShouldTypeNameEndWithDto_WhenDto()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IDto>(configure => configure.WithNameMatch(".*Dto$"))
        .ShouldBeValid();
}
```

####  Type Namespace Matchs

Validate that types implementing an interface have namespaces starting with `MyApp` and ending with `.Dtos` :

```csharp
[Fact]
public void DtoTypes_ShouldBeInDtoNamespace()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IDto>(configure => configure.WithNamespaceMatch(@"^MyApp.*\.Dtos$"))
        .ShouldBeValid();
}
```
### Method Testing

#### Required Method Attribute

```csharp
[Fact]
public void ShouldHaveMyAttribute_WhenImplementingIDto()
{
    AssemblyArchitectureconfigure
        .ForAssembly(Assembly.Load("MyApp"))
        .WithType<IDto>(configure => configure.WithMethod(type => type.MethodA(),
            configureMethod => configureMethod.WithRequiredAttribute(typeof(MyAttribute))))
        .ShouldBeValid();
}