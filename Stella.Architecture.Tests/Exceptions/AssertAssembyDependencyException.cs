using System.Reflection;

namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertAssembyDependencyException(AssemblyName assemblyName)
  : Exception($"Assembly depends on forbidden assembly '{assemblyName.FullName}'")
{
  public AssemblyName ReferencedAssembly { get; } = assemblyName;
}