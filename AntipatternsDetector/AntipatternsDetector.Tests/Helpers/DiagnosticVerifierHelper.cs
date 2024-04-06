using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AntipatternsDetector.Tests.Helpers
{
    public abstract class DiagnosticVerifierHelper
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        private static readonly MetadataReference DataContractReference = MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location);
        
        private static readonly MetadataReference Xunit = MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location);

        private static readonly MetadataReference NetStandard = MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.1.0.0").Location);
        private static readonly MetadataReference Runtime = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=0.0.0.0").Location);
        private static readonly MetadataReference Mscorlib = MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "mscorlib.dll"));


        internal static string DefaultFilePathPrefix = "Test";
        internal static string CSharpDefaultFileExt = "cs";
        internal static string TestProjectName = "TestProject";


        protected internal static Task<Diagnostic[]> GetSortedDiagnosticsAsync(ProjectOptions projectOptions, DiagnosticAnalyzer analyzer)
        {
            return GetSortedDiagnosticsFromDocumentsAsync(analyzer, GetDocuments(projectOptions));
        }

        public static async Task<Diagnostic[]> GetSortedDiagnosticsFromDocumentsAsync(DiagnosticAnalyzer analyzer, Document[] documents)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilationWithAnalyzers = (await project.GetCompilationAsync()).WithAnalyzers(ImmutableArray.Create(analyzer));
                var diags = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
                foreach (var diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        for (int i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = await document.GetSyntaxTreeAsync();
                            if (tree == diag.Location.SourceTree)
                            {
                                diagnostics.Add(diag);
                            }
                        }
                    }
                }
            }

            var results = SortDiagnostics(diagnostics);
            diagnostics.Clear();
            return results;
        }

        private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }


        private static Document[] GetDocuments(ProjectOptions projectOptions)
        {
            var project = CreateProject(projectOptions);
            var documents = project.Documents.ToArray();

            if (projectOptions.Sources.Length != documents.Length)
            {
                throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        public static Document CreateDocument(ProjectOptions projectOptions)
        {
            return CreateProject(projectOptions).Documents.First();
        }


        private static Project CreateProject(ProjectOptions projectOptions)
        {
            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
            var solution = new AdhocWorkspace()
                           .CurrentSolution
                           .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                           .AddMetadataReference(projectId, CorlibReference)
                           .AddMetadataReference(projectId, SystemCoreReference)
                           .AddMetadataReference(projectId, CSharpSymbolsReference)
                           .AddMetadataReference(projectId, Mscorlib)
                           .AddMetadataReference(projectId, NetStandard)
                           .AddMetadataReference(projectId, Runtime)
                           .AddMetadataReference(projectId, CodeAnalysisReference)
                           .AddMetadataReference(projectId, DataContractReference)
                           .AddMetadataReference(projectId, Xunit);

            if (projectOptions.CompilationOptions != null)
            {
                solution = solution.WithProjectCompilationOptions(projectId, projectOptions.CompilationOptions);
            }

            var count = 0;
            foreach (var source in projectOptions.Sources)
            {
                var newFileName = DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }
            return solution.GetProject(projectId);
        }
    }
}

