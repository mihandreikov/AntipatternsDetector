# Roslyn Analyzers for detecting common antipatterns

A set of analyzers which detect common antipatterns in C# code. The analyzers are implemented using the .NET Compiler Platform SDK (Roslyn).

## Content
### AntipatternsDetector
A .NET Standard project with implementations of sample analyzers and code fix providers.

- [NanoServiceAnalyzer.cs](NanoServiceAnalyzer.cs): An analyzer that analyze project on how much API they have. If the API is less than 2, it will report an error.

### AntipatternsDetector.Tests
Unit and integration tests for the analyzers.

### ConsoleApplication
A console application to debug the analyzers and code fix providers on real solutions.

### MuteErrors
A project that references the sample analyzers and suppresses the errors reported by them.
