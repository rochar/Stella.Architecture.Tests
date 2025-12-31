using Newtonsoft.Json;

namespace Stella.Architecture.Tests.Tests.App;

public sealed class AForbiddenDependencyClass(JsonSerializer serializer)
{

    // ReSharper disable once UnusedMember.Global
    public string Property { get; } = JsonConvert.SerializeObject("Hello World");

    // ReSharper disable once UnusedMember.Global
    public JsonSerializer Serializer { get; } = serializer;
}
