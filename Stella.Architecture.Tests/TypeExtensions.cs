namespace Stella.Architecture.Tests;

public static class TypeExtensions
{
    extension(Type type)
    {
        public bool IsRecord()
        {
            return type.GetProperty("EqualityContract",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null;
        }

        public void ShouldBeRecord()
        {
            if (!type.IsRecord())
                throw new AssertArchitectureException($"{type.FullName} expected to be a record");
        }
    }
}