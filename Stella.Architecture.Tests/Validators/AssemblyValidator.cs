using Stella.Architecture.Tests.Exceptions;
using System.Reflection;
using System.Linq;

namespace Stella.Architecture.Tests.Validators;

internal class AssemblyValidator(Assembly assemblyToValidate)
{
    private readonly List<(System.Text.RegularExpressions.Regex regex, string? customErrorMessage)> _forbiddenAssemblyRules = [];
    private readonly HashSet<string> _allowedAssemblyNames = [];
    private IEnumerable<Assembly>? _solutionAssemblies;

    public void WithSolutionContext(IEnumerable<Assembly> solutionAssemblies)
    {
        _solutionAssemblies = solutionAssemblies;
    }

    public void WithAssemblyForbiddenDependency(string regularExpression)
    {
        var regExpression = new System.Text.RegularExpressions.Regex(regularExpression,
            System.Text.RegularExpressions.RegexOptions.Compiled);
        _forbiddenAssemblyRules.Add((regExpression, null));
    }

    public void WithAllowedSolutionDependencies(IEnumerable<string> assemblyNames)
    {
        if (_forbiddenAssemblyRules.Any(r => r.customErrorMessage != null))
        {
            throw new InvalidOperationException("You cannot use both allowed and forbidden solution dependencies.");
        }

        foreach (var name in assemblyNames)
        {
            _allowedAssemblyNames.Add(name);
        }
    }

    public void WithForbiddenSolutionDependencies(IEnumerable<string> assemblyNames)
    {
        if (_allowedAssemblyNames.Any())
        {
            throw new InvalidOperationException("You cannot use both allowed and forbidden solution dependencies.");
        }

        foreach (var name in assemblyNames)
        {
            var regex = new System.Text.RegularExpressions.Regex($"^{System.Text.RegularExpressions.Regex.Escape(name)}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);
            
            _forbiddenAssemblyRules.Add((regex, $"Assembly '{assemblyToValidate.GetName().Name}' is forbidden to depend on solution assembly '{name}'"));
        }
    }

    public IEnumerable<AssertAssembyDependencyException> ShouldBeValid()
    {
        var exceptions = new List<AssertAssembyDependencyException>();

        var referencedAssemblies = assemblyToValidate.GetReferencedAssemblies();

        foreach (var referenced in referencedAssemblies)
        {
            var nameToMatch = referenced.Name ?? referenced.FullName;
            
            foreach (var (regex, customErrorMessage) in _forbiddenAssemblyRules)
            {
                if (regex.IsMatch(nameToMatch))
                {
                    exceptions.Add(customErrorMessage != null 
                        ? new AssertAssembyDependencyException(referenced, customErrorMessage) 
                        : new AssertAssembyDependencyException(referenced));
                }
            }
        }

        if (_solutionAssemblies != null && _allowedAssemblyNames.Any())
        {
            var solutionAssemblyNames = new HashSet<string>(_solutionAssemblies.Select(a => a.GetName().Name));

            foreach (var referenced in referencedAssemblies)
            {
                if (solutionAssemblyNames.Contains(referenced.Name))
                {
                    if (!_allowedAssemblyNames.Contains(referenced.Name))
                    {
                        exceptions.Add(new AssertAssembyDependencyException(referenced,
                            $"Assembly '{assemblyToValidate.GetName().Name}' is not allowed to depend on solution assembly '{referenced.Name}'"));
                    }
                }
            }
        }

        return exceptions;
    }
}