using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace MuteErrors
{
    /// <summary>
    ///     Console application which mute errors by pragma warnings in C# projects
    /// </summary>
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            // var a = MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions{DiscoveryTypes = DiscoveryType.DeveloperConsole|DiscoveryType.VisualStudioSetup, WorkingDirectory = @"C:\src\"}).ToList();
            //MSBuildLocator.RegisterMSBuildPath(@"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin");
            //MSBuildLocator.RegisterDefaults();
            var skippedDiagnostics = new List<Diagnostic>();
            var exceptionDiagnostics = new[] { "CS0246", "CS0234", "CS0103" };

            var projectWithoutDiagnosticsPath =
                Path.Combine(Directory.GetCurrentDirectory(), "ProjectsWithoutDiagnostics.txt");

            // Use while loop, because for muting some diagnostics required more than one iteration
            while (true)
            {
                var projectWithoutDiagnostics = File.Exists(projectWithoutDiagnosticsPath)
                    ? File.ReadAllLines(projectWithoutDiagnosticsPath)
                    : Array.Empty<string>();
                var projectPaths = Directory.GetFiles(@"C:\src\", @"*.csproj",
                    SearchOption.AllDirectories);

                foreach (var projectPath in projectPaths.Except(projectWithoutDiagnostics))
                {
                    using (var workspace = MSBuildWorkspace.Create())
                    {
                        var project = await workspace.OpenProjectAsync(projectPath);

                        var compilation = await project.GetCompilationAsync();
                        var allDiagnostics = compilation.GetDiagnostics()
                            .Where(x => x.Severity == DiagnosticSeverity.Error &&
                                        exceptionDiagnostics.Contains(x.Id) == false)
                            .ToList();

                        var groupedDiagnostics = allDiagnostics
                            .Where(x => x.Location.Kind == LocationKind.SourceFile)
                            .GroupBy(x => x.Location.SourceTree?.FilePath)
                            .ToList();
                        if (allDiagnostics.Any() == false)
                        {
                            Console.WriteLine($"{projectPath} doesn't have any diagnostics");
                            File.AppendAllLines(projectWithoutDiagnosticsPath, new[] { projectPath });
                            continue;
                        }

                        Console.WriteLine($"Project {projectPath} has {allDiagnostics.Count} diagnostics.");

                        foreach (var diagnosticGroup in groupedDiagnostics)
                        {
                            var sourceTree = diagnosticGroup.FirstOrDefault()?.Location.SourceTree;
                            if (sourceTree is null)
                            {
                                Console.Error.WriteLine($"Can not get sourceTree from {diagnosticGroup.Key}");
                                continue;
                            }

                            var sortedDiagnostics = diagnosticGroup
                                .Select(x => (x.Id, x.Location.GetLineSpan().StartLinePosition.Line));

                            var lines = (await File.ReadAllLinesAsync(diagnosticGroup.Key)).ToList();
                            foreach (var sortedDiagnostic in sortedDiagnostics.OrderByDescending(x => x.Line))
                            {
                                lines.Insert(sortedDiagnostic.Line, $"#pragma warning disable {sortedDiagnostic.Id}");
                                lines.Insert(sortedDiagnostic.Line + 2,
                                    $"#pragma warning restore {sortedDiagnostic.Id}");
                            }

                            await File.WriteAllLinesAsync(diagnosticGroup.Key, lines);

                            Console.WriteLine($"File {diagnosticGroup.Key} was saved.");
                        }

                        Console.WriteLine($"Fixed {allDiagnostics.Count} diagnostics in project {projectPath}.");
                    }
                }

                Console.WriteLine("Press Esc to exit or any key to continue.");
                var key = Console.ReadKey();
                if (key is { Key: ConsoleKey.Escape })
                {
                    break;
                }
            }
        }
    }
}