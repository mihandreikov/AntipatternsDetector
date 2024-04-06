using Microsoft.CodeAnalysis.CSharp;

namespace AntipatternsDetector.Tests.Helpers
{
    public class ProjectOptions
    {
        /// <summary>
        ///     Classes in the form of strings
        /// </summary>
        public string[] Sources { get; set; }
            
        /// <summary>
        ///     Compilation options
        /// </summary>
        public CSharpCompilationOptions CompilationOptions { get; set; }
    }
}