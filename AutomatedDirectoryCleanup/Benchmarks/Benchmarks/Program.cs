using BenchmarkDotNet.Running;
using Benchmarks;

// cd into Benchmarks project directory and then run in terminal:
// dotnet run -c Release

BenchmarkRunner.Run<DirectoryCleanerBenchmarks>();