using Stella.Architecture.Tests.Tests.App.Sardine.Atlantic;

namespace Stella.Architecture.Tests.Tests.App.Sardine
{
    internal class Sardine(AtlanticSardine atlanticSardine)
    {
        private readonly AtlanticSardine _atlanticSardine = atlanticSardine;
    }
}
