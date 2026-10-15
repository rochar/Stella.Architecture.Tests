using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Stella.Architecture.Tests.Tests.App.Tuna;
using System.Collections.Concurrent;
using System.Reflection;

namespace Stella.Architecture.Tests.Tests;

public class TypeDependenciesCacheConcurrencyTests
{
    private const int WorkersPerAssembly = 4;
    private static readonly TimeSpan JoinTimeout = TimeSpan.FromMinutes(1);

    [Fact]
    public void ShouldBeValidWhenAssembliesAreAnalysedInParallel()
    {
        // Large assemblies no other test analyses, so the process-wide type cache is cold for them regardless of
        // test order, and many threads fill it while others read it.
        Assembly[] assemblies =
        [
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(System.Xml.XmlDocument).Assembly,
            typeof(System.Text.Json.JsonSerializer).Assembly,
            typeof(HttpClient).Assembly,
            typeof(System.Data.DataTable).Assembly,
            typeof(System.Linq.Expressions.Expression).Assembly,
            typeof(JsonConvert).Assembly,
            typeof(Substitute).Assembly,
            typeof(Castle.DynamicProxy.ProxyGenerator).Assembly,
        ];
        var errors = new ConcurrentBag<Exception>();
        using var start = new ManualResetEventSlim();

        var threads = assemblies
            .SelectMany(assembly => Enumerable.Repeat(assembly, WorkersPerAssembly))
            .Select(assembly => new Thread(() =>
            {
                start.Wait();
                try
                {
                    AssemblyArchitectureBuilder.ForAssembly(assembly)
                        .WithDependencyUsedOnly<Tuna>()
                        .ShouldBeValid();
                }
                catch (Exception exception)
                {
                    errors.Add(exception);
                }
            })
            { IsBackground = true })
            .ToList();

        threads.ForEach(thread => thread.Start());
        start.Set();

        // Bounded wait so a deadlock in the cache fails the test instead of hanging the run.
        threads.ShouldAllBe(thread => thread.Join(JoinTimeout));
        errors.ShouldBeEmpty();
    }
}
