namespace Stella.Architecture.Tests.Tests.App.Tuna.Atlantic
{
    internal sealed class AtlanticTuna
    {
        public Salmon.Salmon Salmon { get; } = new();

        public override string ToString()
        {
            return Salmon.ToString() ?? string.Empty;
        }
    }
}
