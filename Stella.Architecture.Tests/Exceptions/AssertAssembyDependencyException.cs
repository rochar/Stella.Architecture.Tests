using System.Reflection;

namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertAssembyDependencyException(AssemblyName assemblyName, string message)
    : Exception(message)
{
    public AssertAssembyDependencyException(AssemblyName assemblyName) : this(assemblyName,
        $"Assembly depends on forbidden assembly '{assemblyName.FullName}'")
    {
    }

    public AssemblyName ReferencedAssembly { get; } = assemblyName;
}