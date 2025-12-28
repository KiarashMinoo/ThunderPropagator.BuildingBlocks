using BenchmarkDotNet.Running;
using ThunderPropagator.UnitTests;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();