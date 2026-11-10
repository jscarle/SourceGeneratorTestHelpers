using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorTestHelpers.Tests.Fixtures;

internal sealed class SnapshotGenerator(string? source, DiagnosticSeverity? diagnosticSeverity = null) : IIncrementalGenerator
{
    internal static GeneratorDriverRunResult Run(string? source, DiagnosticSeverity? diagnosticSeverity = null)
    {
        var generator = new SnapshotGenerator(source, diagnosticSeverity);
        var compilation = CSharpCompilation.Create("SnapshotTests");
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator());
        var completedDriver = driver.RunGenerators(compilation);
        return completedDriver.GetRunResult();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, GenerateSource);
    }

    private void GenerateSource(SourceProductionContext context, Compilation _)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (source is not null)
        {
            var generatedSource = SourceText.From(source, Encoding.UTF8);
            context.AddSource("TestId.g.cs", generatedSource);
        }

        if (diagnosticSeverity.HasValue)
        {
            var descriptor = new DiagnosticDescriptor("SNAP001", "Snapshot test", "Snapshot generator diagnostic", "Tests", diagnosticSeverity.Value, true);
            var diagnostic = Diagnostic.Create(descriptor, Location.None);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
