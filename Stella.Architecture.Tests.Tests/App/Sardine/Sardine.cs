using Stella.Architecture.Tests.Tests.App.Sardine.Atlantic;

namespace Stella.Architecture.Tests.Tests.App.Sardine
{
    internal sealed class Sardine(AtlanticSardine atlanticSardine)
    {
        // ReSharper disable once UnusedMember.Local
        private readonly AtlanticSardine _atlanticSardine = atlanticSardine;
    }
}
