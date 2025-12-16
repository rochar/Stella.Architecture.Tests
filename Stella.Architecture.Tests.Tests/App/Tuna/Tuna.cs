namespace Stella.Architecture.Tests.Tests.App.Tuna
{
    public class Tuna
    {
        public string? Salmon { get; }

        public Tuna(Salmon.Salmon salmon)
        {
            ArgumentNullException.ThrowIfNull(salmon);

            Salmon = salmon.ToString();
        }
    }

    public class TunaField
    {
        private readonly Salmon.Salmon _salmon = new();

        public override string ToString()
        {
            return _salmon.ToString() ?? string.Empty;
        }
    }

    public class TunaProperty
    {
        public Salmon.Salmon Salmon { get; } = new();

        public override string ToString()
        {
            return Salmon.ToString() ?? string.Empty;
        }
    }
}
