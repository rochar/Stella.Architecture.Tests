using Stella.Architecture.Tests.Exceptions;
using System.Reflection;

namespace Stella.Architecture.Tests.Validators;

internal class AssemblyValidator
{
    private readonly List<System.Text.RegularExpressions.Regex> _forbiddenAssemblyDependencyRegularExpressions = [];

    public void WithAssemblyForbiddenDependency(string regularExpression)
    {
        var regExpression = new System.Text.RegularExpressions.Regex(regularExpression,
            System.Text.RegularExpressions.RegexOptions.Compiled);
        _forbiddenAssemblyDependencyRegularExpressions.Add(regExpression);
    }

    public IEnumerable<AssertAssembyDependencyException> ShouldBeValid(Assembly assembly)
    {
        if (!_forbiddenAssemblyDependencyRegularExpressions.Any())
            return [];

        return assembly.GetReferencedAssemblies()
            .Where(a => _forbiddenAssemblyDependencyRegularExpressions.Any(r => r.IsMatch(a.FullName ?? a.Name)))
            .Select(a => new AssertAssembyDependencyException(a));
    }
}