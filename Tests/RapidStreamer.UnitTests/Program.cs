using BenchmarkDotNet.Running;
using RapidStreamer.UnitTests;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();