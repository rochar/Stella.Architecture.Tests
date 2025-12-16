namespace Stella.Architecture.Tests.Tests.App
{
    public class DependsOnTuna
    {
        public string? Tuna { get; }

        public DependsOnTuna(Tuna.Tuna tuna)
        {
            ArgumentNullException.ThrowIfNull(tuna);

            Tuna = tuna.ToString();
        }
    }
}